using Overimagined.Common;
using UnityEngine;

public class BodyRenderer : SingletonMono<BodyRenderer>
{
    public Mesh mesh;
    public Material material;

    internal void RenderActive(MotionState[] array)
    {
        var mArray = new Matrix4x4[array.Length];
        for (int i = 0; i < mArray.Length; i++)
        {
            var pose = array[i].Pose;
            mArray[i] = Matrix4x4.TRS(pose.Position, pose.Orientation, new Vector3(0.5f, 1, 3));
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, mArray, mArray.Length);
    }
}
