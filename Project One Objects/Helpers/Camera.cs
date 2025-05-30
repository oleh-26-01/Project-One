﻿using System.Numerics;

namespace Project_One_Objects.Helpers;

public class Camera
{
    private Vector2 _center;
    private Func<Vector2>? _followPositionGetter;
    private Action? _updateZoomLabel;
    private float _zoom;

    public Camera(Vector2 center, Vector2 position, float zoom = 1)
    {
        _center = center;
        Position = position;
        _zoom = zoom;
    }

    public Vector2 Position { get; set; }

    /// <summary>Zoom level of the camera.</summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <remarks>1 is default zoom level.</remarks>
    public float Zoom
    {
        get => _zoom;
        set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Zoom must be positive");

            _zoom = value;
            _updateZoomLabel?.Invoke();
        }
    }

    /// <summary>Centering relative coordinates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Vector2 Center
    {
        get => _center;
        set
        {
            if (value.X < 0 || value.Y < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Center must be positive");

            _center = value;
        }
    }

    public bool IsFollowing => _followPositionGetter != null;

    public void Follow(Func<Vector2> positionGetter)
    {
        _followPositionGetter = positionGetter;
    }

    public void FollowStop()
    {
        _followPositionGetter = null;
    }

    public void FollowUpdate(float dt)
    {
        if (_followPositionGetter == null) return;

        var followPosition = _followPositionGetter();
        Position += (followPosition - Center - Position).Scale(dt / 0.2);
    }

    public void SetZoomUpdater(Action? updateZoomLabel)
    {
        _updateZoomLabel = updateZoomLabel;
    }

    public void Move(Vector2 direction)
    {
        Position += direction;
    }

    /// <summary>Moves the camera synchronously with the screen refresh.</summary>
    /// <param name="direction">"left", "right", "up", "down".</param>
    /// <param name="timeDiff">time difference in milliseconds between last update and current update.</param>
    /// <param name="coeff">multiplier for camera move.</param>
    public void MoveSync(Vector2 direction, float timeDiff, float coeff = 1f)
    {
        var vectorDirection = direction * coeff * timeDiff / 1000f;
        vectorDirection /= _zoom;
        Move(vectorDirection);
    }

    /// <summary>Increases zoom by the given value.</summary>
    /// <param name="value">percent as float from 0 to 1 or more</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ZoomIn(float value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than 0");

        _zoom *= 1 + value;
        _updateZoomLabel?.Invoke();
    }

    /// <summary>Decreases zoom by the given value.</summary>
    /// <param name="value">percent as float from 0 to 1</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ZoomOut(float value)
    {
        if (value is <= 0 or >= 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 1");

        _zoom *= 1 - value;
        _updateZoomLabel?.Invoke();
    }

    /// <summary>Convert a point from world space to screen space.</summary>
    /// <param name="point">Point in world space</param>
    /// <returns>Point in screen space</returns>
    public Vector2 ConvertIn(Vector2 point)
    {
        var result = point;
        result -= _center;
        result /= _zoom;
        result += _center + Position;
        return result;
    }

    /// <summary>Convert an array of points from world space to screen space.</summary>
    /// <param name="points">Array of points to convert.</param>
    /// <returns>New array with converted points.</returns>
    public Vector2[] ConvertIn(Vector2[] points)
    {
        var result = new Vector2[points.Length];
        points.CopyTo(result, 0);
        for (var i = 0; i < points.Length; i++)
        {
            result[i] -= _center;
            result[i] /= _zoom;
            result[i] += _center + Position;
        }

        return result;
    }

    /// <summary>Convert a list of points from world space to screen space.</summary>
    /// <param name="points">List of points to convert.</param>
    /// <returns>New List with converted points.</returns>
    public List<Vector2> ConvertIn(List<Vector2> points)
    {
        List<Vector2> result = new(points);
        for (var i = 0; i < points.Count; i++)
        {
            result[i] -= _center;
            result[i] /= _zoom;
            result[i] += _center + Position;
        }

        return result;
    }

    /// <summary>Convert a point from absolute value to camera relative value.</summary>
    /// <param name="point">Point in absolute value.</param>
    /// <returns>Point in camera relative value.</returns>
    public Vector2 ConvertOut(Vector2 point)
    {
        var result = point;
        result -= Position + _center;
        result *= _zoom;
        result += _center;
        return result;
    }

    /// <summary>Convert an array of points from absolute values to camera relative values.</summary>
    /// <param name="points">Array of points to convert.</param>
    /// <returns>New array with converted points.</returns>
    public Vector2[] ConvertOut(Vector2[] points)
    {
        var result = new Vector2[points.Length];
        points.CopyTo(result, 0);
        for (var i = 0; i < points.Length; i++)
        {
            result[i] -= Position + _center;
            result[i] *= _zoom;
            result[i] += _center;
        }

        return result;
    }

    /// <summary>Convert a list of points from absolute values to camera relative values.</summary>
    /// <param name="points">List of points to convert.</param>
    /// <returns>New List with converted points.</returns>
    public List<Vector2> ConvertOut(List<Vector2> points)
    {
        List<Vector2> result = new(points);
        for (var i = 0; i < points.Count; i++)
        {
            result[i] -= Position + _center;
            result[i] *= _zoom;
            result[i] += _center;
        }

        return result;
    }
}