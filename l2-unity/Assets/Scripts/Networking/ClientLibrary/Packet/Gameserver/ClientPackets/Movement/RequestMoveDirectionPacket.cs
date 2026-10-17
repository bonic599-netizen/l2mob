using UnityEngine;
using System;
public class RequestMoveDirectionPacket : ClientPacket
{
    public RequestMoveDirectionPacket(Vector3 direction, int heading, float verticalVelocity, Vector3 position, bool requireResponse) : base(0x01)
    {
        int origX = (int)(position.z * 52.5f);
        int origY = (int)(position.x * 52.5f);
        int origZ = (int)(position.y * 52.5f);
        int destX = (int)((position.z + direction.z * 3f) * 52.5f);
        int destY = (int)((position.x + direction.x * 3f) * 52.5f);
        int destZ = origZ;

        WriteI(destX);
        WriteI(destY);
        WriteI(destZ);
        WriteI(origX);
        WriteI(origY);
        WriteI(origZ);
        WriteI(1); // mouse/movement flag
        BuildPacket();
    }
}
