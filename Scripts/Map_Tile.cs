using UnityEngine;

public class Map_Tile : MonoBehaviour
{
    public Sprite[] tileSprites;
    public Sprite SetTileSprite(string _dir, out float _angle)
    {
        Sprite sprite = null;
        float angle = 0;
        if (_dir[1] == 'O')// 좌 있고
        {
            if (_dir[3] == 'O')// 아래 있고
            {
                if (_dir[4] == 'O') // 위 있고
                {
                    if (_dir[6] == 'O')// 오른 있고
                    {
                        // 대각선 위치
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')// 주변 전부 잇음
                                    {
                                        sprite = tileSprites[14];
                                    }
                                    else
                                    {
                                        angle = -90f;
                                        sprite = tileSprites[13];
                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[13];
                                    }
                                    else
                                    {
                                        angle = -90f;
                                        sprite = tileSprites[11];
                                    }
                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        sprite = tileSprites[13];
                                    }
                                    else
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[11];
                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        sprite = tileSprites[11];
                                    }
                                    else
                                    {
                                        sprite = tileSprites[10];
                                    }
                                }
                            }
                        }
                        else// 0 없음
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[11];
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[11];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    else// 좌 있고 아래 있고 위 있고 오른 없고
                    {
                        if (_dir[0] == 'O')
                        {
                            angle = -90f;
                            sprite = tileSprites[9];
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {

                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = -90f;
                                        sprite = tileSprites[4];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
                else // 좌 있고 아래 있고 위 없음
                {
                    if (_dir[6] == 'O')// 우 있음
                    {
                        if (_dir[0] == 'O')
                        {
                            angle = 0f;
                            sprite = tileSprites[9];
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                        angle = 0f;
                                        sprite = tileSprites[8];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[4];
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        if (_dir[0] == 'O')
                        {
                            angle = -90f;
                            sprite = tileSprites[5];
                        }
                        else
                        {
                            angle = -90f;
                            sprite = tileSprites[2];
                        }
                    }
                }
            }
            else // 좌측 있고 아래 없음
            {
                if (_dir[4] == 'O') // 위쪽 있음
                {
                    if (_dir[6] == 'O')// 오른 잇음
                    {
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[9];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[4];
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                        else// 0 없고
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[9];
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[4];
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    else// 오른 없고
                    {
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                angle = 180f;
                                sprite = tileSprites[5];
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[2];
                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[2];
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[5];
                                    }
                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 180f;
                                        sprite = tileSprites[2];
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_dir[6] == 'O')
                    {
                        angle = 0f;
                        sprite = tileSprites[3];
                    }
                    else
                    {
                        angle = -90f;
                        sprite = tileSprites[1];
                    }
                }
            }
        }
        else// 좌측 없음
        {
            if (_dir[3] == 'O')// 아래 있고
            {
                if (_dir[4] == 'O')// 위 있고
                {
                    if (_dir[6] == 'O')// 오른 있고
                    {
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[4];
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {

                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[9];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    else// 오른쪽 없음
                    {
                        angle = 90f;
                        sprite = tileSprites[3];
                    }
                }
                else// 위 없고
                {
                    if (_dir[6] == 'O')// 오른 있고
                    {
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {

                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[5];
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[2];
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[5];
                                    }
                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {

                                    }
                                    else
                                    {
                                        angle = 0f;
                                        sprite = tileSprites[2];
                                    }
                                }
                            }
                        }
                    }
                    else// 오른쪽 없음
                    {
                        angle = 0f;
                        sprite = tileSprites[1];
                    }
                }
            }
            else// 아래 없음
            {
                if (_dir[4] == 'O')// 위 있고
                {
                    if (_dir[6] == 'O')// 오른 있고
                    {
                        if (_dir[0] == 'O')
                        {

                        }
                        else
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[5];
                                    }
                                    else
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[2];
                                    }
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                if (_dir[5] == 'O')
                                {

                                }
                                else
                                {
                                    if (_dir[7] == 'O')
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[5];
                                    }
                                    else
                                    {
                                        angle = 90f;
                                        sprite = tileSprites[2];
                                    }
                                }
                            }
                        }

                    }
                    else// 오른쪽 없음
                    {
                        angle = 180f;
                        sprite = tileSprites[1];
                    }
                }
                else// 위없음
                {
                    if (_dir[6] == 'O')
                    {
                        angle = 90f;
                        sprite = tileSprites[1];
                    }
                    else// 오른쪽 없음
                    {
                        angle = 0f;
                        sprite = tileSprites[0];
                    }
                }
            }
        }
        _angle = angle;
        return sprite;
    }
}
