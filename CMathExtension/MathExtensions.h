#pragma once
struct vector2
{
    float x;
    float y;
};

extern "C" __declspec(dllexport) void insertion_sort(vector2 * array, int length);
