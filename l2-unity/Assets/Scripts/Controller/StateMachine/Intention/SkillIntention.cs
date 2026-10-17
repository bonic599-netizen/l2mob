using UnityEngine;

public class SkillIntention : IntentionBase
{
    private static SkillInfo _lastSkillIntention = null;
    private static Skillgrp _lastSkillIntentionGrp = null;
    public static bool CtrlPressed = false;
    public static bool ShiftPressed = false;

    public SkillIntention(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter(object arg0)
    {
        if (arg0 != null)
        {
            _lastSkillIntention = (SkillInfo)arg0;
            CtrlPressed = InputManager.Instance.Ctrl;
            ShiftPressed = InputManager.Instance.Shift;
            _lastSkillIntentionGrp = SkillgrpTable.Instance.GetSkill(_lastSkillIntention.Id, _lastSkillIntention.Level);
        }

        if (_lastSkillIntention == null)
        {
            return;
        }

        if (_lastSkillIntentionGrp == null)
        {
            _lastSkillIntentionGrp = SkillgrpTable.Instance.GetSkill(_lastSkillIntention.Id, 1);
        }

        if (SkillRequiresTarget()) // target required?
        {
            Transform target = TargetManager.Instance.Target?.transform;
            if (target == null)
            {
                if (SkillIsABuffOrHeal())
                {
                    TargetManager.Instance.SetTarget(PlayerEntity.Instance);
                }
                else
                {
                    // Send skill request anyway in case it's a self/aura/toggle skill with positive CastRange in table
                    _stateMachine.ChangeState(PlayerState.IDLE);
                    _stateMachine.NotifyEvent(Event.READY_TO_SKILL, _lastSkillIntention);
                    return;
                }
            }

            int rawCastRange = _lastSkillIntentionGrp.CastRange > 0 ? _lastSkillIntentionGrp.CastRange : 60;
            float skillRange = Mathf.Max(rawCastRange / 52.5f * 0.95f, 1.5f); //5% error margin, min 1.5m
            Entity targetEntity = TargetManager.Instance.Target;
            Vector3 targetPos = targetEntity.transform.position;

            float distance = Vector3.Distance(VectorUtils.To2D(PlayerEntity.Instance.transform.position), VectorUtils.To2D(targetPos));

            // Debug.Log($"Distance: {distance} SkillRange: {skillRange}");
            if (distance <= skillRange && !_stateMachine.WaitingForServerReply)
            {
                Debug.Log("Using skill on target");

                _stateMachine.ChangeState(PlayerState.IDLE);

                if (!targetEntity.IsDead)
                {
                    _stateMachine.NotifyEvent(Event.READY_TO_SKILL, _lastSkillIntention);
                }
            }
            else
            {
                // Move to target with a 5% error margin
                PathFinderController.Instance.MoveTo(targetPos, skillRange, () =>
                {
                    _stateMachine.ChangeIntention(Intention.INTENTION_FOLLOW, MoveReason.SKILL);
                });
            }
        }
        else
        {
            _stateMachine.NotifyEvent(Event.READY_TO_SKILL, _lastSkillIntention);
        }
    }

    private bool SkillRequiresTarget()
    {
        Skillgrp skillgrp = _lastSkillIntentionGrp;
        bool hasRange = skillgrp.CastRange != -1;
        // bool isDamage = skillgrp.IconType == SkillIconType.Physical || skillgrp.IconType == SkillIconType.Magic;
        // bool isDebuff = skillgrp.IconType == SkillIconType.Debuff;
        // bool isBuff = skillgrp.IconType == SkillIconType.Buff;

        bool result = hasRange;// && (isDamage || isDebuff || isBuff);

        if (result)
        {
            Debug.Log($"Skill {_lastSkillIntentionGrp.Id} requires a target.");
        }
        else
        {
            Debug.Log($"Skill {_lastSkillIntentionGrp.Id} doesn't require a target.");
        }

        return result;
    }

    private bool SkillIsABuffOrHeal()
    {
        Skillgrp skillgrp = _lastSkillIntentionGrp;
        bool isBuff = skillgrp.IconType == SkillType.Buff;
        bool isHeal = skillgrp.OperateType == SkillOperateType.Heal;
        //TODO: 
        // Create an operate type for heals so that they can behave like buffs 
        // (auto target self when no target selected)
        bool result = isBuff || isHeal;
        if (result)
        {
            Debug.Log($"Skill {_lastSkillIntentionGrp.Id} is a buff/heal.");
        }
        else
        {
            Debug.Log($"Skill {_lastSkillIntentionGrp.Id} isn't a buff/heal.");
        }

        return result;
    }

    public override void Exit() { }
    public override void Update()
    {

    }
}