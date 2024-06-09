using Unity.VisualScripting;
using UnityEngine;

public class Utilities
{
    #region downToOne

    //rounds the numbers to -1, 0, 1

    public static Vector3 DownToOne(Vector3 toRound)
    {
        Vector3 rounded = new Vector3(0, 0, 0);

        for (int i = 0; i < 3; i++)
        {
            if (toRound[i] > 0)
            {
                rounded[i] = 1;
            }
            else if (toRound[i] < 0)
            {
                rounded[i] = -1;
            }
            else
            {
                rounded[i] = 0;
            }

        }

        return rounded;
    }

    public static float DownToOne(float toRound)
    {
        float rounded;

        if (toRound > 0)
        {
            rounded = 1;
        }
        else if (toRound < 0)
        {
            rounded = -1;
        }
        else
        {
            rounded = 0;
        }

        return rounded;
    }

    public static int DownToOne(int toRound)
    {
        int rounded;

        if (toRound > 0)
        {
            rounded = 1;
        }
        else if (toRound < 0)
        {
            rounded = -1;
        }
        else
        {
            rounded = 0;
        }

        return rounded;
    }

    #endregion downToOne

    #region moved

    public static bool Moved(float one, float two, float lastOne, float lastTwo, float maxDistance)
    {
        if (one >= lastOne + maxDistance)
        {
            return true;
        }
        else if (one <= lastOne - maxDistance)
        {
            return true;
        }
        else if (two >= lastTwo + maxDistance)
        {
            return true;
        }
        else if (two <= lastTwo - maxDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool Moved(Vector2 current, Vector2 previous, float maxDistance)
    {
        if (current.x >= previous.x + maxDistance)
        {
            return true;
        }
        else if (current.x <= previous.x - maxDistance)
        {
            return true;
        }
        else if (current.y >= previous.y + maxDistance)
        {
            return true;
        }
        else if (current.y <= previous.y - maxDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion moved

    #region Vector3manipulation

    public static Vector3 RemoveY(Vector3 toRemove)
    {
        return new Vector3(toRemove.x, 0f, toRemove.z);
    }

    public static Vector3 RemoveY(Vector3 toRemove, bool removeOrNot)
    {
        if (removeOrNot)
        {
            return new Vector3(toRemove.x, 0f, toRemove.z);
        }
        else
        {
            return toRemove;
        }
    }

    public static Vector3 InsertY(Vector3 toInsertTo, float toInsert)
    {
        return new Vector3(toInsertTo.x, toInsert, toInsertTo.z);
    }



    #endregion Vector3manipulation
}