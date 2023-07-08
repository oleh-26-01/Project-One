#include "pch.h"
#include "MathExtensions.h"

void insertion_sort(vector2* array, int length)
{
    for (int i = 1; i < length; i++)
    {
        vector2 key = array[i];
        int j = i - 1;

        while (j >= 0 && array[j].y > key.y)
        {
            array[j + 1] = array[j];
            j--;
        }

        array[j + 1] = key;
    }
}
