using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace SteelBallRun.Entities.Collider;

public class CircleCollider : Collider
{
    public Vector2 Center { get; set; }
    public float Radius { get; }

    public CircleCollider(float radius)
    {
        Radius = radius;
    }

    public bool Intersects(CircleCollider other)
    {
        float distance = Vector2.Distance(Center, other.Center);
        return distance < Radius + other.Radius;
    }

    public bool Intersects(RectangleF rectangle)
    {
        float closestX = Math.Clamp(Center.X, rectangle.Left, rectangle.Right);
        float closestY = Math.Clamp(Center.Y, rectangle.Top, rectangle.Bottom);

        Vector2 closestPoint = new Vector2(closestX, closestY);

        return Vector2.Distance(Center, closestPoint) < Radius;
    }
}