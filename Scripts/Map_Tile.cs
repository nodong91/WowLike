using UnityEngine;

public class Map_Tile : MonoBehaviour
{
    public Sprite[] tileSprites;
    public Sprite SetTileSprite(string _dir, out float _angle)
    {
        Sprite sprite = null;
        float angle = 0;
        if (_dir[1] == 'O')// �� �ְ�
        {
            if (_dir[3] == 'O')// �Ʒ� �ְ�
            {
                if (_dir[4] == 'O') // �� �ְ�
                {
                    if (_dir[6] == 'O')// ���� �ְ�
                    {
                        // �밢�� ��ġ
                        if (_dir[0] == 'O')
                        {
                            if (_dir[2] == 'O')
                            {
                                if (_dir[5] == 'O')
                                {
                                    if (_dir[7] == 'O')// �ֺ� ���� ����
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
                        else// 0 ����
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
                    else// �� �ְ� �Ʒ� �ְ� �� �ְ� ���� ����
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
                else // �� �ְ� �Ʒ� �ְ� �� ����
                {
                    if (_dir[6] == 'O')// �� ����
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
            else // ���� �ְ� �Ʒ� ����
            {
                if (_dir[4] == 'O') // ���� ����
                {
                    if (_dir[6] == 'O')// ���� ����
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
                        else// 0 ����
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
                    else// ���� ����
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
        else// ���� ����
        {
            if (_dir[3] == 'O')// �Ʒ� �ְ�
            {
                if (_dir[4] == 'O')// �� �ְ�
                {
                    if (_dir[6] == 'O')// ���� �ְ�
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
                    else// ������ ����
                    {
                        angle = 90f;
                        sprite = tileSprites[3];
                    }
                }
                else// �� ����
                {
                    if (_dir[6] == 'O')// ���� �ְ�
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
                    else// ������ ����
                    {
                        angle = 0f;
                        sprite = tileSprites[1];
                    }
                }
            }
            else// �Ʒ� ����
            {
                if (_dir[4] == 'O')// �� �ְ�
                {
                    if (_dir[6] == 'O')// ���� �ְ�
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
                    else// ������ ����
                    {
                        angle = 180f;
                        sprite = tileSprites[1];
                    }
                }
                else// ������
                {
                    if (_dir[6] == 'O')
                    {
                        angle = 90f;
                        sprite = tileSprites[1];
                    }
                    else// ������ ����
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
