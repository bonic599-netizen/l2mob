using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    private EventProcessor _eventProcessor;
    private WorldSpawner _worldSpawner;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask _entityMask;
    [SerializeField] private LayerMask _simpleEntityMask;
    [SerializeField] private LayerMask _entityClickAreaMask;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private LayerMask _clickThroughMask;
    [SerializeField] private LayerMask _groundMask;

    [SerializeField] private bool _offlineMode = false;

    public bool OfflineMode { get { return _offlineMode; } }
    public LayerMask GroundMask { get { return _groundMask; } }

    private static World _instance;
    public static World Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }

        _eventProcessor = EventProcessor.Instance;
        _worldSpawner = GetComponent<WorldSpawner>();
    }

    void OnDestroy()
    {
        _instance = null;
    }

    void Start()
    {
        UpdateMasks();
        DisableUndergroundFog();
    }

    private void DisableUndergroundFog()
    {
        GameObject fogGo = GameObject.Find("Height Fog Global");
        if (fogGo != null)
        {
            fogGo.SetActive(false);
        }
    }

    void UpdateMasks()
    {
        NameplatesManagerGame.Instance.SetMask(_entityMask);
        Geodata.Instance.ObstacleMask = _obstacleMask;
        ClickManager.Instance.SetMasks(_entityClickAreaMask, _clickThroughMask);
        CameraController.Instance.SetMask(_obstacleMask);
        TargetManager.Instance.SetMask(_simpleEntityMask);
    }

    private GameObject _fallbackGround;
    private GameObject _fallbackPlazaDecor;
    private float _playerAnchorY = float.NaN;
    private Vector3 _lastServerPlayerPos;

    public void SetPlayerGroundAnchor(Vector3 playerPos)
    {
        _playerAnchorY = playerPos.y;
        _lastServerPlayerPos = playerPos;
        DisableUndergroundFog();
        if (!HasRealTerrainAt(playerPos))
        {
            EnsureFallbackGround(playerPos, true);
        }
    }

    private bool HasRealTerrainAt(Vector3 pos)
    {
        RaycastHit[] hits = Physics.RaycastAll(pos + Vector3.up * 80f, Vector3.down, 200f, _groundMask);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && hits[i].collider.gameObject != _fallbackGround)
            {
                return true;
            }
        }
        return false;
    }

    public float GetGroundHeight(Vector3 pos)
    {
        RaycastHit[] nearHits = Physics.RaycastAll(pos + Vector3.up * 1.5f, Vector3.down, 3.5f, _groundMask);
        for (int i = 0; i < nearHits.Length; i++)
        {
            if (nearHits[i].collider != null && nearHits[i].collider.gameObject != _fallbackGround)
            {
                return nearHits[i].point.y;
            }
        }

        RaycastHit[] farHits = Physics.RaycastAll(pos + Vector3.up * 80f, Vector3.down, 200f, _groundMask);
        for (int i = 0; i < farHits.Length; i++)
        {
            if (farHits[i].collider != null && farHits[i].collider.gameObject != _fallbackGround)
            {
                return farHits[i].point.y;
            }
        }

        float targetY = !float.IsNaN(_playerAnchorY) ? _playerAnchorY : pos.y;
        EnsureFallbackGround(new Vector3(pos.x, targetY, pos.z), float.IsNaN(_playerAnchorY));
        return targetY;
    }

    private void EnsureFallbackGround(Vector3 pos, bool allowHeightUpdate = false)
    {
        DisableUndergroundFog();

        float surfaceY = !float.IsNaN(_playerAnchorY) ? _playerAnchorY : pos.y;
        bool created = false;

        if (_fallbackGround == null)
        {
            _fallbackGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _fallbackGround.name = "HellsingFallbackGround";
            _fallbackGround.transform.localScale = new Vector3(4000f, 4f, 4000f);
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                _fallbackGround.layer = groundLayer;
            }
            var mr = _fallbackGround.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                if (lit != null)
                {
                    Material mat = new Material(lit);
                    mat.color = new Color(0.64f, 0.60f, 0.54f);
                    mat.SetFloat("_Smoothness", 0.12f);
                    Texture2D tex = Resources.Load<Texture2D>("Data/Textures/field_deco_T/d_ston01");
                    if (tex != null)
                    {
                        mat.SetTexture("_BaseMap", tex);
                        mat.mainTextureScale = new Vector2(600f, 600f);
                    }
                    mr.sharedMaterial = mat;
                }
            }
            created = true;
        }

        if (created || allowHeightUpdate ||
            Vector2.Distance(new Vector2(_fallbackGround.transform.position.x, _fallbackGround.transform.position.z), new Vector2(pos.x, pos.z)) > 400f ||
            Mathf.Abs((_fallbackGround.transform.position.y + 2f) - surfaceY) > 0.05f)
        {
            _fallbackGround.transform.position = new Vector3(pos.x, surfaceY - 2f, pos.z);
            BuildFallbackTownLandmarks(new Vector3(pos.x, surfaceY, pos.z));
            Physics.SyncTransforms();
        }
    }

    private void BuildFallbackTownLandmarks(Vector3 center)
    {
        if (_fallbackPlazaDecor == null)
        {
            _fallbackPlazaDecor = new GameObject("HellsingTownPlazaLandmarks");
        }
        _fallbackPlazaDecor.transform.position = center;

        if (_fallbackPlazaDecor.transform.childCount > 0)
        {
            return;
        }

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        Material marbleMat = lit != null ? new Material(lit) { color = new Color(0.78f, 0.75f, 0.68f) } : null;
        Material goldMat = lit != null ? new Material(lit) { color = new Color(0.85f, 0.68f, 0.28f) } : null;

        // Central plaza dais ring
        GameObject dais = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dais.name = "PlazaCenterRing";
        dais.transform.SetParent(_fallbackPlazaDecor.transform, false);
        dais.transform.localPosition = new Vector3(0f, -0.08f, 0f);
        dais.transform.localScale = new Vector3(18f, 0.1f, 18f);
        Destroy(dais.GetComponent<Collider>());
        if (marbleMat != null) dais.GetComponent<MeshRenderer>().sharedMaterial = marbleMat;

        // 8 colonnade pillars around the town square for spatial orientation
        int pillarCount = 8;
        float radius = 16f;
        for (int i = 0; i < pillarCount; i++)
        {
            float angle = i * Mathf.PI * 2f / pillarCount;
            Vector3 pPos = new Vector3(Mathf.Cos(angle) * radius, 2.5f, Mathf.Sin(angle) * radius);

            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"TownPillar_{i}";
            pillar.transform.SetParent(_fallbackPlazaDecor.transform, false);
            pillar.transform.localPosition = pPos;
            pillar.transform.localScale = new Vector3(0.9f, 2.5f, 0.9f);
            Destroy(pillar.GetComponent<Collider>());
            if (marbleMat != null) pillar.GetComponent<MeshRenderer>().sharedMaterial = marbleMat;

            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = $"TownBeacon_{i}";
            cap.transform.SetParent(_fallbackPlazaDecor.transform, false);
            cap.transform.localPosition = pPos + new Vector3(0f, 2.8f, 0f);
            cap.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            Destroy(cap.GetComponent<Collider>());
            if (goldMat != null) cap.GetComponent<MeshRenderer>().sharedMaterial = goldMat;
        }
    }

    public string GetRegionName(Vector3 unityPos)
    {
        return LocationTable.Instance.GetRegionName(unityPos);
    }

    public void ToggleWorldMap(int categoryId = 0)
    {
        if (MapWindow.Instance != null)
        {
            MapWindow.Instance.OpenCategory(categoryId);
            MapWindow.Instance.ToggleHideWindow();
            return;
        }

        if (NpcHtmlWindow.Instance == null) return;

        Vector3 pos = PlayerEntity.Instance != null ? PlayerEntity.Instance.transform.position : _lastServerPlayerPos;
        int l2X = Mathf.RoundToInt(pos.z * 52.5f);
        int l2Y = Mathf.RoundToInt(pos.x * 52.5f);
        int l2Z = Mathf.RoundToInt(pos.y * 52.5f);
        string region = GetRegionName(pos);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("<html><title>World Map & Navigation</title><body><center>");
        sb.Append($"<font color=\"LEVEL\">{region}</font><br1>");
        sb.Append($"Coords: X={l2X} Y={l2Y} Z={l2Z}<br>");

        if (categoryId == 0)
        {
            sb.Append("<table width=270 bgcolor=000000>");
            sb.Append("<tr><td width=135 align=center><button value=\"Gatekeeper Teleport\" action=\"bypass -h hellsing_map_gk\" width=125 height=20 back=\"L2UI_ch3.Btn1_normalOn\" fore=\"L2UI_ch3.Btn1_normal\"></td>");
            sb.Append("<td width=135 align=center><button value=\"Nearby NPCs\" action=\"bypass -h hellsing_map_nearby\" width=125 height=20 back=\"L2UI_ch3.Btn1_normalOn\" fore=\"L2UI_ch3.Btn1_normal\"></td></tr>");
            sb.Append("<tr><td width=135 align=center><button value=\"3D Talking Island\" action=\"bypass -h hellsing_map_preview_ti\" width=125 height=20 back=\"L2UI_ch3.Btn1_normalOn\" fore=\"L2UI_ch3.Btn1_normal\"></td>");
            sb.Append("<td width=135 align=center><button value=\"Return Server Pos\" action=\"bypass -h hellsing_map_return_server\" width=125 height=20 back=\"L2UI_ch3.Btn1_normalOn\" fore=\"L2UI_ch3.Btn1_normal\"></td></tr>");
            sb.Append("</table><br>");

            sb.Append("<font color=\"LEVEL\">-- World Map Categories --</font><br1>");
            sb.Append("<table width=270>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_1\">1. Города и деревни (19)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_2\">2. Замки и территории (18)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_3\">3. Стартовые локации (12)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_4\">4. Локации Gludio и Gludin (30)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_5\">5. Локации Dion и Floran (20)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_6\">6. Локации возле Giran (16)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_7\">7. Локации Oren и Hunters (25)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_8\">8. Локации возле Aden (29)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_9\">9. Локации возле Goddard (25)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_10\">10. Локации возле Rune (15)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_11\">11. Локации возле Schuttgart (12)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_12\">12. Катакомбы и некрополи (14)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_13\">13. Подземелья и спец. зоны (21)</a></td></tr>");
            sb.Append("<tr><td><a action=\"bypass -h hellsing_map_cat_14\">14. Острова и гавани (9)</a></td></tr>");
            sb.Append("</table>");
        }
        else if (categoryId == 999) // Nearby NPCs view
        {
            sb.Append("<table width=270><tr><td><button value=\"< Back to Map\" action=\"bypass -h hellsing_map_cat_0\" width=100 height=18 back=\"L2UI_ch3.Btn1_normalOn\" fore=\"L2UI_ch3.Btn1_normal\"></td></tr></table><br>");
            sb.Append("<font color=\"LEVEL\">Nearby Town NPCs (Click to Target):</font><br1>");

            Entity[] allEntities = FindObjectsByType<Entity>(FindObjectsSortMode.None);
            int shown = 0;
            for (int i = 0; i < allEntities.Length && shown < 20; i++)
            {
                Entity ent = allEntities[i];
                if (ent == null || ent == PlayerEntity.Instance || ent.Identity == null) continue;
                float dist = Vector3.Distance(pos, ent.transform.position);
                if (dist > 150f) continue;

                string title = !string.IsNullOrEmpty(ent.Identity.Title) ? $" ({ent.Identity.Title})" : "";
                sb.Append($"<a action=\"bypass -h hellsing_map_target_{ent.Identity.Id}\">{ent.Identity.Name}{title} [{Mathf.RoundToInt(dist)}m]</a><br1>");
                shown++;
            }

            if (shown == 0)
            {
                sb.Append("No NPCs within 150m.<br1>");
            }
        }
        else
        {
            LocationCategory cat = (LocationCategory)categoryId;
            List<LocationPoint> locList = LocationTable.Instance.GetLocations(cat);
            string catTitle = locList.Count > 0 ? locList[0].CategoryTitle : $"Category {categoryId}";

            sb.Append("<table width=270><tr><td><button value=\"< Back to Map\" action=\"bypass -h hellsing_map_cat_0\" width=100 height=18 back=\"L2UI_ch3.Btn1_normalOn\" fore=\"L2UI_ch3.Btn1_normal\"></td>");
            sb.Append($"<td align=right><font color=\"LEVEL\">{catTitle}</font></td></tr></table><br>");

            sb.Append("<table width=270>");
            for (int i = 0; i < locList.Count; i++)
            {
                var loc = locList[i];
                float dist = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(loc.UnityPos.x, loc.UnityPos.z));
                string distStr = dist > 1000f ? $"{dist / 1000f:F1}km" : $"{Mathf.RoundToInt(dist)}m";
                int lx = Mathf.RoundToInt(loc.L2Pos.x);
                int ly = Mathf.RoundToInt(loc.L2Pos.y);
                int lz = Mathf.RoundToInt(loc.L2Pos.z);

                sb.Append($"<tr><td><a action=\"bypass -h hellsing_map_goto_{lx}_{ly}_{lz}\">{i + 1}. {loc.Name}</a></td><td align=right><font color=\"AAAAAA\">{distStr}</font></td></tr>");
            }
            sb.Append("</table>");
        }

        sb.Append("</center></body></html>");
        NpcHtmlWindow.Instance.RefreshContent(0, sb.ToString(), 0);
    }

    public void HandleMapAction(string action)
    {
        if (action == "hellsing_map_refresh")
        {
            ToggleWorldMap(0);
            return;
        }

        if (action == "hellsing_map_nearby")
        {
            ToggleWorldMap(999);
            return;
        }

        if (action.StartsWith("hellsing_map_cat_"))
        {
            string catStr = action.Substring("hellsing_map_cat_".Length);
            if (int.TryParse(catStr, out int catId))
            {
                ToggleWorldMap(catId);
                return;
            }
        }

        if (action.StartsWith("hellsing_map_goto_"))
        {
            string[] parts = action.Substring("hellsing_map_goto_".Length).Split('_');
            if (parts.Length == 3 &&
                float.TryParse(parts[0], out float lx) &&
                float.TryParse(parts[1], out float ly) &&
                float.TryParse(parts[2], out float lz))
            {
                Vector3 targetUnityPos = new Vector3(ly / 52.5f, lz / 52.5f, lx / 52.5f);
                SetPlayerGroundAnchor(targetUnityPos);
                targetUnityPos.y = GetGroundHeight(targetUnityPos);

                if (PlayerTransformReceive.Instance != null)
                {
                    PlayerTransformReceive.Instance.SetNewPosition(targetUnityPos);
                }
                else if (PlayerEntity.Instance != null)
                {
                    PlayerEntity.Instance.transform.position = targetUnityPos;
                }

                NpcHtmlWindow.Instance.HideWindow(false);
                return;
            }
        }

        if (action == "hellsing_map_preview_ti")
        {
            // Talking Island Village center on the loaded 17_25 3D terrain
            Vector3 tiPos = new Vector3(244510f / 52.5f, -3720f / 52.5f, -84168f / 52.5f);
            SetPlayerGroundAnchor(tiPos);
            tiPos.y = GetGroundHeight(tiPos) + 0.1f;
            if (PlayerTransformReceive.Instance != null)
            {
                PlayerTransformReceive.Instance.SetNewPosition(tiPos);
            }
            else if (PlayerEntity.Instance != null)
            {
                PlayerEntity.Instance.transform.position = tiPos;
            }
            NpcHtmlWindow.Instance.HideWindow(false);
            return;
        }

        if (action == "hellsing_map_return_server")
        {
            if (_lastServerPlayerPos != Vector3.zero)
            {
                SetPlayerGroundAnchor(_lastServerPlayerPos);
                if (PlayerTransformReceive.Instance != null)
                {
                    PlayerTransformReceive.Instance.SetNewPosition(_lastServerPlayerPos);
                }
                else if (PlayerEntity.Instance != null)
                {
                    PlayerEntity.Instance.transform.position = _lastServerPlayerPos;
                }
            }
            NpcHtmlWindow.Instance.HideWindow(false);
            return;
        }

        if (action == "hellsing_map_gk")
        {
            Entity[] allEntities = FindObjectsByType<Entity>(FindObjectsSortMode.None);
            Entity bestGk = null;
            float bestDist = float.MaxValue;
            Vector3 pPos = PlayerEntity.Instance != null ? PlayerEntity.Instance.transform.position : Vector3.zero;

            for (int i = 0; i < allEntities.Length; i++)
            {
                Entity ent = allEntities[i];
                if (ent == null || ent == PlayerEntity.Instance || ent.Identity == null) continue;
                string title = (ent.Identity.Title ?? "").ToLowerInvariant();
                string name = (ent.Identity.Name ?? "").ToLowerInvariant();
                if (title.Contains("gatekeeper") || name.Contains("trisha") || name.Contains("clarissa") || name.Contains("roxxy") ||
                    (ent.Identity.NpcId >= 30059 && ent.Identity.NpcId <= 30080))
                {
                    float d = Vector3.Distance(pPos, ent.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestGk = ent;
                    }
                }
            }

            if (bestGk != null)
            {
                SelectAndInteractNpc(bestGk);
            }
            else
            {
                // Send request for gatekeeper or unstuck
                GameClient.Instance?.ClientPacketHandler?.RequestBypassToServer("bypass -h npc_0_Chat 1");
            }
            return;
        }

        if (action.StartsWith("hellsing_map_target_"))
        {
            string idStr = action.Substring("hellsing_map_target_".Length);
            if (int.TryParse(idStr, out int targetObjId))
            {
                Entity[] allEntities = FindObjectsByType<Entity>(FindObjectsSortMode.None);
                for (int i = 0; i < allEntities.Length; i++)
                {
                    if (allEntities[i] != null && allEntities[i].Identity != null && allEntities[i].Identity.Id == targetObjId)
                    {
                        SelectAndInteractNpc(allEntities[i]);
                        break;
                    }
                }
            }
        }
    }

    private void SelectAndInteractNpc(Entity npc)
    {
        if (npc == null) return;
        TargetManager.Instance.SetTarget(npc);
        if (GameClient.Instance?.ClientPacketHandler != null)
        {
            GameClient.Instance.ClientPacketHandler.SendRequestAction(npc.Identity.Id);
            GameClient.Instance.ClientPacketHandler.SendRequestAction(npc.Identity.Id);
        }
    }

    public Task UpdateObjectPosition(int id, Vector3 position)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            position.y = GetGroundHeight(position);
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
        });
    }

    public Task AdjustObjectPositionAndRotation(int id, Vector3 position, int heading)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            if (id == GameClient.Instance.CurrentPlayerId)
            {
                SetPlayerGroundAnchor(position);
            }
            position.y = GetGroundHeight(position);

            e.Identity.Position = position;
            e.Identity.Heading = heading;

            float rotation = VectorUtils.ConvertRotToUnity(heading);
            if (id == GameClient.Instance.CurrentPlayerId)
            {
                PlayerTransformReceive.Instance.SetNewPosition(position);
                NetworkCharacterControllerShare.Instance.Heading = heading;
            }
            else
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(rotation);
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
            }
        });
    }

    public Task NpcHtmlReceived(int objectId, string html, int itemId)
    {
        _eventProcessor.QueueEvent(() => NpcHtmlWindow.Instance.RefreshContent(objectId, html, itemId));
        if (objectId == 0)
        {
            return Task.CompletedTask;
        }
        return _worldSpawner.ExecuteWithEntityAsync(objectId, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.LookAt(PlayerEntity.Instance.transform);
        });
    }

    public Task UpdateObjectRotation(int id, float angle)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(angle);
        });
    }

    public Task UpdateObjectDestination(int id, Vector3 currentPosition, Vector3 destination)
    {
        if (id == GameClient.Instance.CurrentPlayerId)
        {
            return Task.CompletedTask;
        }
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            currentPosition.y = GetGroundHeight(currentPosition);
            destination.y = GetGroundHeight(destination);
            e.Identity.Position = currentPosition;
            StartCoroutine(HandleUpdateDestination(e, currentPosition, destination));
        });
    }

    IEnumerator HandleUpdateDestination(Entity e, Vector3 currentPosition, Vector3 destination)
    {
        // Debug.LogWarning("Entity move to destination: " + destination);

        NetworkEntityReferenceHolder referenceHolder = (NetworkEntityReferenceHolder)e.ReferenceHolder;
        //sync current position with server
        referenceHolder.NetworkTransformReceive.ResumePositionSync();
        referenceHolder.NetworkTransformReceive.SetNewPosition(currentPosition);

        //wait for position to be updated
        yield return new WaitForFixedUpdate();

        //tell the entity to move to location
        referenceHolder.NetworkTransformReceive.PausePositionSync();

        if (e.Combat.AttackTarget == null)
        {
            referenceHolder.NetworkCharacterControllerReceive.SetDestination(destination, 0);

            //set the entity expected position to destination
            referenceHolder.NetworkTransformReceive.SetNewPosition(destination);
        }
        else
        {
            referenceHolder.NetworkCharacterControllerReceive.SetDestination(destination, WorldCombat.Instance.GetRealAttackRange(e, e.Combat.AttackTarget));
        }

        //look at destination
        referenceHolder.NetworkTransformReceive.LookAt(destination);
    }

    public Task UpdateObjectMoveDirection(int id, Vector3 position, Vector3 direction, float verticalVelocity, long timestamp)
    {

        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            position.y = GetGroundHeight(position);
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkCharacterControllerReceive.UpdateMoveDirection(position, direction, verticalVelocity, timestamp);
        });
    }

    public Task ObjectStoppedMove(int id, Vector3 position, int heading)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            Debug.LogWarning($"[{e.transform.name}] ObjectStoppedMove");
            if (id == GameClient.Instance.CurrentPlayerId)
            {
                SetPlayerGroundAnchor(position);
            }
            position.y = GetGroundHeight(position);
            e.Identity.Position = position;
            e.Identity.Heading = heading;

            float rotation = VectorUtils.ConvertRotToUnity(heading);

            if (id == GameClient.Instance.CurrentPlayerId)
            {
                PlayerTransformReceive.Instance.SetNewPosition(position);
            }
            else if (e.Identity.EntityType == EntityType.User)
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
            }
            else
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(rotation);
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkCharacterControllerReceive.ResetDestination();
            }
        });
    }

    public Task ChangeWaitType(int owner, ChangeWaitTypePacket.WaitType moveType, Vector3 entityPosition)
    {
        return _worldSpawner.ExecuteWithEntityAsync(owner, e =>
        {
            entityPosition.y = GetGroundHeight(entityPosition);
            if (owner != GameClient.Instance.CurrentPlayerId)
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(entityPosition);
            }
            else
            {
                PlayerTransformReceive.Instance.SetNewPosition(entityPosition);
            }

            e.UpdateWaitType(moveType);
        });
    }

    public Task ChangeMoveType(int owner, bool running)
    {
        return _worldSpawner.ExecuteWithEntityAsync(owner, e =>
                {
                    e.UpdateMoveType(running);
                });
    }

    public Task EntityTeleported(int entityId, Vector3 teleportTo, bool loadingScreen)
    {
        return _worldSpawner.ExecuteWithEntityAsync(entityId, e =>
        {
            if (entityId != GameClient.Instance.CurrentPlayerId)
            {
                teleportTo.y = GetGroundHeight(teleportTo);
                e.Identity.Position = teleportTo;
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(teleportTo);
            }
            else
            {
                SetPlayerGroundAnchor(teleportTo);
                teleportTo.y = GetGroundHeight(teleportTo);
                e.Identity.Position = teleportTo;
                if (GameClient.Instance.PlayerInfo.Identity != null)
                {
                    GameClient.Instance.PlayerInfo.Identity.Position = teleportTo;
                }
                GameManager.Instance.NotifyEvent(GameEvent.TELEPORTING);
                GameClient.Instance.ClientPacketHandler?.NotifyAppearing();
            }
        });
    }

    public Task SocialActionReceived(int objectId, int action)
    {
        return _worldSpawner.ExecuteWithEntityAsync(objectId, e =>
        {
            if (action == SocialActionPacket.LEVELUP_ACTION)
            {
                Debug.Log("Entity level up!");
                WorldCombat.Instance.EntityCastSkill(e, 2122);
            }
            else
            {
                e.AnimationController.Emote(action);
            }
        });
    }

    public void DestroyWorld()
    {
        ProjectileManager.Instance.DestroyAllProjectiles();
        ParticleManager.Instance.DestroyAllParticles();
        WorldSpawner.Instance.DestroyEntities();
        WorldSpawner.Instance.ClearEntities();
    }
}
