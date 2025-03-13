using UnityEngine;
using UnityEngine.UI;

public class Map_Tile : MonoBehaviour
{
    public Sprite[] tileSprites;
    string direction;

    //========================================================================================================
    public void SetTile(Image _image, string _dir)
    {
        direction = _dir;
        Sprite sprite = null;
        float angle = 0f;
        if (direction[1] == 'O')// �� ����
        {
            if (direction[3] == 'O')// �Ʒ� ����
            {
                if (direction[4] == 'O')// �� ����
                {
                    if (direction[6] == 'O')// ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[13];
                                break;

                            case 1:
                                if (direction[0] == 'O') angle = 0f;
                                if (direction[2] == 'O') angle = 270f;
                                if (direction[5] == 'O') angle = 90f;
                                if (direction[7] == 'O') angle = 180f;
                                sprite = tileSprites[12];
                                break;

                            case 2:
                                if (direction[0] == 'O')
                                {
                                    if (direction[2] == 'O') angle = 270f;
                                    if (direction[5] == 'O') angle = 0f;
                                    if (direction[7] == 'O') angle = 0f;
                                }
                                else if (direction[2] == 'O')
                                {
                                    if (direction[5] == 'O') angle = 90f;
                                    if (direction[7] == 'O') angle = 180f;
                                }
                                else if (direction[5] == 'O')
                                {
                                    if (direction[7] == 'O') angle = 90f;
                                }
                                // �ؽ�ó
                                if ((direction[0] == 'O' && direction[7] == 'O') ||
                                    (direction[2] == 'O' && direction[5] == 'O'))
                                {
                                    sprite = tileSprites[11];
                                }
                                else
                                {
                                    sprite = tileSprites[10];
                                }
                                break;

                            case 3:
                                if (direction[0] != 'O') angle = 90f;
                                if (direction[2] != 'O') angle = 0f;
                                if (direction[5] != 'O') angle = 180f;
                                if (direction[7] != 'O') angle = 270f;
                                sprite = tileSprites[9];
                                break;

                            case 4:// ��� ���� ��
                                sprite = tileSprites[14];
                                break;
                        }
                    }
                    else// ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        angle = 270f;
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[7];
                                break;

                            case 1:
                                if (direction[0] == 'O') sprite = tileSprites[5];
                                else if (direction[2] == 'O') sprite = tileSprites[6];
                                else sprite = tileSprites[7];
                                break;

                            default:
                                if (direction[0] == 'O')
                                {
                                    if (direction[2] == 'O') sprite = tileSprites[4];
                                    else sprite = tileSprites[5];
                                }
                                else if (direction[2] == 'O')
                                {
                                    sprite = tileSprites[6];
                                }
                                else
                                {
                                    sprite = tileSprites[7];
                                }
                                break;
                        }
                    }
                }
                else// ������
                {
                    if (direction[6] == 'O')// �� ���� ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[7];
                                break;

                            case 1:
                                if (direction[0] == 'O') sprite = tileSprites[6];
                                else if (direction[5] == 'O') sprite = tileSprites[5];
                                else sprite = tileSprites[7];
                                break;

                            default:
                                if (direction[0] == 'O')
                                {
                                    if (direction[5] == 'O') sprite = tileSprites[4];
                                    else sprite = tileSprites[6];
                                }
                                else if (direction[5] == 'O')
                                {
                                    sprite = tileSprites[5];
                                }
                                else
                                {
                                    sprite = tileSprites[7];
                                }
                                break;

                        }
                    }
                    else// �� ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        angle = 270f;
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[3];
                                break;

                            default:
                                if (direction[0] == 'O') sprite = tileSprites[2];
                                else sprite = tileSprites[3];
                                break;
                        }
                    }
                }
            }
            else// �Ʒ� ����
            {
                if (direction[4] == 'O')// �� ����
                {
                    if (direction[6] == 'O')// ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        angle = 180f;
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[7];
                                break;

                            case 1:
                                if (direction[2] == 'O') sprite = tileSprites[5];
                                else if (direction[7] == 'O') sprite = tileSprites[6];
                                else sprite = tileSprites[7];
                                break;

                            default:
                                if (direction[2] == 'O')
                                {
                                    if (direction[7] == 'O') sprite = tileSprites[4];
                                    else sprite = tileSprites[5];
                                }
                                else if (direction[7] == 'O')
                                {
                                    sprite = tileSprites[6];
                                }
                                else
                                {
                                    sprite = tileSprites[7];
                                }
                                break;
                        }
                    }
                    else // �� �ְ� �Ʒ� ���� ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        angle = 180f;
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[3];
                                break;

                            default:
                                if (direction[2] == 'O') sprite = tileSprites[2];
                                else sprite = tileSprites[3];
                                break;
                        }
                    }
                }
                else// �� �ְ� �� ���� �Ʒ� ����
                {
                    if (direction[6] == 'O')// ���� ����
                    {
                        angle = 0f;
                        sprite = tileSprites[8];
                    }
                    else // ���� ����
                    {
                        angle = 270f;
                        sprite = tileSprites[1];
                    }
                }
            }
        }
        else// �� ����
        {
            if (direction[3] == 'O')// �Ʒ� ����
            {
                if (direction[4] == 'O')// �� ����
                {
                    if (direction[6] == 'O')// ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        angle = 90f;
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[7];
                                break;

                            case 1:
                                if (direction[5] == 'O') sprite = tileSprites[6];
                                else if (direction[7] == 'O') sprite = tileSprites[5];
                                else sprite = tileSprites[7];
                                break;

                            default:
                                if (direction[5] == 'O')
                                {
                                    if (direction[7] == 'O') sprite = tileSprites[4];
                                    else sprite = tileSprites[6];
                                }
                                else if (direction[7] == 'O')
                                {
                                    sprite = tileSprites[5];
                                }
                                else
                                {
                                    sprite = tileSprites[7];
                                }
                                break;
                        }
                    }
                    else // ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        angle = 90f;
                        sprite = tileSprites[8];
                    }
                }
                else// �� ���� �� ����
                {
                    if (direction[6] == 'O')// ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        angle = 0f;
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[3];
                                break;

                            default:
                                if (direction[5] == 'O') sprite = tileSprites[2];
                                else sprite = tileSprites[3];
                                break;
                        }
                    }
                    else// �Ʒ���
                    {
                        angle = 0f;
                        sprite = tileSprites[1];
                    }
                }
            }
            else// �� ���� �Ʒ� ����
            {
                if (direction[4] == 'O')// �� �ְ�
                {
                    if (direction[6] == 'O')// ���� ����
                    {
                        int index = OpenIndex(out bool _connect, out int _angleType);
                        angle = 90f;
                        switch (index)
                        {
                            case 0:// ��� ���� ��
                                sprite = tileSprites[3];
                                break;

                            default:
                                if (direction[7] == 'O') sprite = tileSprites[2];
                                else sprite = tileSprites[3];
                                break;
                        }
                    }
                    else// �� ��
                    {
                        angle = 180f;
                        sprite = tileSprites[1];
                    }
                }
                else// �� �Ʒ� �� ����
                {
                    if (direction[6] == 'O')// ���� ����
                    {
                        angle = 90f;
                        sprite = tileSprites[1];
                    }
                    else// �ֺ� ����
                    {
                        sprite = tileSprites[0];
                    }
                }
            }
        }
        _image.sprite = sprite;
        _image.transform.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    int OpenIndex(out bool _connect, out int _angleType)
    {
        int index = 0;
        bool connect = false;
        int angleType = 0;
        if (direction[0] == 'O')
        {
            connect = (direction[2] == 'O' || direction[5] == 'O');
            angleType = 0;
            index++;
        }
        if (direction[2] == 'O')
        {
            connect = (direction[0] == 'O' || direction[7] == 'O');
            angleType = 1;
            index++;
        }
        if (direction[5] == 'O')
        {
            connect = (direction[0] == 'O' || direction[7] == 'O');
            angleType = 2;
            index++;
        }
        if (direction[7] == 'O')
        {
            connect = (direction[2] == 'O' || direction[5] == 'O');
            angleType = 3;
            index++;
        }
        _connect = connect;
        _angleType = angleType;
        return index;
    }
}
