using Godot;
using System;
using System.Collections.Generic;

public static class Utility
{
    public const float HalfPI = Mathf.Pi / 2.1f; //idk why this is 2.1

    public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float delta)
    {
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, float.PositiveInfinity, delta);
    }
    public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        float output_x = 0f;
        float output_y = 0f;
        float output_z = 0f;

        // Based on Game Programming Gems 4 Chapter 1.10
        smoothTime = Mathf.Max(0.0001F, smoothTime);
        float omega = 2F / smoothTime;

        float x = omega * deltaTime;
        float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);

        float change_x = current.X - target.X;
        float change_y = current.Y - target.Y;
        float change_z = current.Z - target.Z;
        Vector3 originalTo = target;

        // Clamp maximum speed
        float maxChange = maxSpeed * smoothTime;

        float maxChangeSq = maxChange * maxChange;
        float sqrmag = change_x * change_x + change_y * change_y + change_z * change_z;
        if (sqrmag > maxChangeSq)
        {
            var mag = (float)Math.Sqrt(sqrmag);
            change_x = change_x / mag * maxChange;
            change_y = change_y / mag * maxChange;
            change_z = change_z / mag * maxChange;
        }

        target.X = current.X - change_x;
        target.Y = current.Y - change_y;
        target.Z = current.Z - change_z;

        float temp_x = (currentVelocity.X + omega * change_x) * deltaTime;
        float temp_y = (currentVelocity.Y + omega * change_y) * deltaTime;
        float temp_z = (currentVelocity.Z + omega * change_z) * deltaTime;

        currentVelocity.X = (currentVelocity.X - omega * temp_x) * exp;
        currentVelocity.Y = (currentVelocity.Y - omega * temp_y) * exp;
        currentVelocity.Z = (currentVelocity.Z - omega * temp_z) * exp;

        output_x = target.X + (change_x + temp_x) * exp;
        output_y = target.Y + (change_y + temp_y) * exp;
        output_z = target.Z + (change_z + temp_z) * exp;

        // Prevent overshooting
        float origMinusCurrent_x = originalTo.X - current.X;
        float origMinusCurrent_y = originalTo.Y - current.Y;
        float origMinusCurrent_z = originalTo.Z - current.Z;
        float outMinusOrig_x = output_x - originalTo.X;
        float outMinusOrig_y = output_y - originalTo.Y;
        float outMinusOrig_z = output_z - originalTo.Z;

        if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y + origMinusCurrent_z * outMinusOrig_z > 0)
        {
            output_x = originalTo.X;
            output_y = originalTo.Y;
            output_z = originalTo.Z;

            currentVelocity.X = (output_x - originalTo.X) / deltaTime;
            currentVelocity.Y = (output_y - originalTo.Y) / deltaTime;
            currentVelocity.Z = (output_z - originalTo.Z) / deltaTime;
        }

        return new Vector3(output_x, output_y, output_z);
    }

    public enum Easing { Fast, Faster, Fastest, Slow, Slower, Slowest, Smooth, Smoother, Smoothest, Bounce, MirroredBounce, Linear };

    public static float EvaulateCurve(Easing easing, float x)
    {
        x = Mathf.Clamp(x, 0f, 1f);
        switch (easing)
        {
            case Easing.Fast:
                return 1 - (1 - x) * (1 - x);
            case Easing.Faster:
                return 1 - Mathf.Pow(1 - x, 4);
            case Easing.Fastest:
                return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
            case Easing.Slow:
                return x * x;
            case Easing.Slower:
                return x * x * x * x;
            case Easing.Slowest:
                return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
            case Easing.Smooth:
                return x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
            case Easing.Smoother:
                return x < 0.5 ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2;
            case Easing.Smoothest:
                return x == 0 ? 0 : x == 1  ? 1 : x < 0.5 ? Mathf.Pow(2, 20 * x - 10) / 2  : (2 - Mathf.Pow(2, -20 * x + 10)) / 2;
            case Easing.Bounce:
                return EaseOutBounce(x);
            case Easing.MirroredBounce:
                return x < 0.5f ? (1f - EaseOutBounce(1f - 2f * x)) / 2f : (1f + EaseOutBounce(2f * x - 1f)) / 2f;
            case Easing.Linear:
                return x;
        }
        return -1f;
    }
    private static float EaseOutBounce(float x)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (x < 1 / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }
    public static List<Node> GetAllChildren(Node node, List<Node> arry = null) 
    {
        if(arry == null)
        { arry = new List<Node>(); }
        arry.Add(node);
        foreach(Node child in node.GetChildren())
        { arry = GetAllChildren(child, arry); }
        return arry;
    }
    public static List<T> GetAllNodesOfType<T>(List<Node> arry) where T : Node
    {
        List<T> output = new List<T>();
        T node;
        foreach(Node n in arry)
        {
            node = n as T;
            if(node != null)
            { output.Add(node); }
        }
        return output;
    }
    private static Node parentNode;
    public static T GetNodeInParent<T>(Node node) where T : Node
    {
        T foundNode;
        parentNode = node.GetParentOrNull<Node>();
        if(parentNode == null)
        { return default(T); } //out of bounds of the tree
        foundNode = parentNode as T;
        if(foundNode == null)
        { return GetNodeInParent<T>(parentNode); } //recursive time
        else
        { return foundNode; } //found it
    }
    static Transform3D tempTransform;
    public static void QuaternionLerp(Node3D obj, Node3D startNode, Vector3 endRotation, float t)
    {
        tempTransform = obj.Transform;
        endRotation.X = Mathf.DegToRad(endRotation.X);
        endRotation.Y = Mathf.DegToRad(endRotation.Y);
        endRotation.Z = Mathf.DegToRad(endRotation.Z);
		tempTransform.Basis = new Basis(startNode.Basis.GetRotationQuaternion().Normalized().Slerp(Quaternion.FromEuler(endRotation).Normalized(), t));
        //tempTransform.Basis.Scale = obj.Scale; //preserve scale cuz goofy ahh
		obj.Transform = tempTransform;
    }
    public static void QuaternionLerp(Node3D obj, Node3D startNode, Node3D endNode, float t)
    {
        QuaternionLerp(obj, startNode.Transform, endNode.Transform, t); //i havent properly tested this yet
        // tempTransform = obj.Transform;
		// tempTransform.Basis = new Basis(startNode.Basis.GetRotationQuaternion().Normalized().Slerp(endNode.Basis.GetRotationQuaternion().Normalized(), t));
        // //tempTransform.Basis.Scale = obj.Scale; //preserve scale cuz goofy ahh
		// obj.Transform = tempTransform;
    }
    public static void QuaternionLerp(Node3D obj, Transform3D startTransform, Transform3D endTransform, float t)
    {
        tempTransform = startTransform;
        tempTransform.Basis = new Basis(startTransform.Basis.GetRotationQuaternion().Normalized().Slerp(endTransform.Basis.GetRotationQuaternion().Normalized(), t));
        obj.Transform = tempTransform;
    }

    static RandomNumberGenerator rng;
    public static float RandomRange(float min, float max)
    {
        if(rng == null)
        { rng = new RandomNumberGenerator(); }

        return rng.RandfRange(min, max);
    }
    public static int RandomRange(int min, int max)
    {
        if(rng == null)
        { rng = new RandomNumberGenerator(); }

        return rng.RandiRange(min, max - 1);
    }
    static Vector3 posBefore;
    static Vector3 rotationBefore;
    static Vector3 scaleBefore;
    static bool wasInsideTree;
    public static void ChangeParent(Node3D node, Node newParent)
    {
        if(node.GetParent() == newParent)
        { return; }

        wasInsideTree = node.IsInsideTree();

        if (wasInsideTree)
        {
            posBefore = node.GlobalPosition;
            rotationBefore = node.GlobalRotation;
            scaleBefore = node.Scale;
            node.GetParent().RemoveChild(node);
        }

        newParent.AddChild(node);

        if(wasInsideTree)
        {
            node.GlobalPosition = posBefore;
            node.GlobalRotation = rotationBefore;
            node.Scale = scaleBefore;
        }
    }

    public static uint GetCollisionLayer(int layer)
    {
        return (uint)Math.Pow(2, layer - 1);
    }
}
