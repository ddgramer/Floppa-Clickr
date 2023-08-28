using UnityEngine;

public class Utilities : MonoBehaviour
{
    #region downToOne
    //rounds the numbers to -1, 0, 1

    public static Vector3 DownToOne(Vector3 toRound)
    {
        Vector3 rounded = new Vector3(0, 0, 0);

        if (toRound.x > 0)
        {
            rounded.x = 1;
        }
        else if (toRound.x < 0)
        {
            rounded.x = -1;
        }
        else
        {
            rounded.x = 0;
        }

        if (toRound.y > 0)
        {
            rounded.y = 1;
        }
        else if (toRound.y < 0)
        {
            rounded.y = -1;
        }
        else
        {
            rounded.y = 0;
        }

        if (toRound.z > 0)
        {
            rounded.z = 1;
        }
        else if (toRound.z < 0)
        {
            rounded.z = -1;
        }
        else
        {
            rounded.z = 0;
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
    #endregion

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
    #endregion
}
