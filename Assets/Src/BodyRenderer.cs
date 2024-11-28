using Overimagined.Common;
using UnityEngine;

public class BodyRenderer : SingletonMono<BodyRenderer>
{
    public Mesh mesh;
    public Material material;
    public Vector3 scale;
    Matrix4x4[] buffer;


    public void RenderActive(MotionState[] array)
    {
        if (buffer == null || buffer.Length < array.Length)
            buffer = new Matrix4x4[array.Length];

        for (int i = 0; i < array.Length; i++)
        {
            var pose = array[i].Pose;
            buffer[i] = Matrix4x4.TRS(pose.Position, pose.Orientation, scale);
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, buffer, array.Length);
    }
}
