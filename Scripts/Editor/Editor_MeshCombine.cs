using System.Collections.Generic;
using UnityEngine;
using System.IO;



#if UNITY_EDITOR
using UnityEditor;
//[CustomEditor(typeof(Editor_MeshCombine))]
//public class MeshCombine_Editor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        GUIStyle fontStyle = new GUIStyle(GUI.skin.button);
//        fontStyle.fontSize = 15;
//        fontStyle.normal.textColor = Color.yellow;

//        Editor_MeshCombine Inspector = target as Editor_MeshCombine;
//        if (GUILayout.Button("Combine", fontStyle, GUILayout.Height(30f)))
//        {
//            Inspector.SetMeshCombine();
//            EditorUtility.SetDirty(Inspector);
//        }
//        GUILayout.Space(10f);

//        base.OnInspectorGUI();
//    }
//}
#endif
namespace P01.Editor
{
    public class Editor_MeshCombine : EditorWindow
    {
        [MenuItem("Graphics Tool/05. MeshCombine")]
        public static void OpenWindow()
        {
            Editor_MeshCombine window = EditorWindow.GetWindow<Editor_MeshCombine>("Editor_MeshCombine");
            window.minSize = new Vector2(500f, 200f);
            window.Show();
        }

        [System.Serializable]
        public class Batch
        {
            [HideInInspector] public string name;
            public Mesh mesh;
            public Material[] materials;
            public List<Matrix4x4> matrix;

            public void AddMatrix(Matrix4x4 _matrix)
            {
                if (matrix == null)
                    matrix = new List<Matrix4x4>();
                if (matrix.Contains(_matrix) == false)
                    matrix.Add(_matrix);
            }
        }
        Dictionary<string, Batch> batchDictionary = new Dictionary<string, Batch>();
        const string folderPath = "Assets/";
        string filePath = "Temp";
        bool tutorialToggle;
        Vector2 scrollPosition;
        int oldIndex, newIndex;

        void OnGUI()
        {
            GUIStyle fontStyle = new()
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleLeft
            };

            GUIStyle buttonStyle = new(GUI.skin.button)
            {
                fontSize = 13,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            };
            if (GUILayout.Button("����", buttonStyle, GUILayout.Height(20f)))
            {
                tutorialToggle = !tutorialToggle;
            }
            ShowTutorial(fontStyle);

            GUILayout.BeginHorizontal("box");
            string path = folderPath + filePath + "/";
            GUILayout.Label(path, fontStyle);
            filePath = GUILayout.TextField(filePath);
            GUILayout.EndHorizontal();

            GameObject[] go = Selection.gameObjects;
            if (go.Length == 0)
                return;

            fontStyle.fontStyle = FontStyle.Bold;
            fontStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Combine Object", fontStyle);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < go.Length; i++)
            {
                EditorGUILayout.ObjectField(go[i], typeof(GameObject), false);
            }
            GUILayout.EndScrollView();
            GUILayout.Label($"{oldIndex} -> {newIndex}", fontStyle);

            buttonStyle.fontSize = 18;
            buttonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button("Combine", buttonStyle, GUILayout.Height(30f)))
            {
                SetMeshCombine();
            }
        }

        void ShowTutorial(GUIStyle _guiText)
        {
            if (tutorialToggle == true)
            {
                _guiText.normal.textColor = Color.gray;
                GUILayout.Space(10f);
                GUILayout.Label(" 1. Assets/~ ���� ������ ��ġ ����", _guiText);
                GUILayout.Label(" 2. ��ġ���� ������Ʈ�� Hierarchy���� ����", _guiText);
                GUILayout.Label(" 3. Combine Object���� ������Ʈ Ȯ��", _guiText);
                GUILayout.Label(" 4. Combine ��ư Ŭ��", _guiText);

                GUILayout.Space(10f);
            }
        }

        public void SetMeshCombine()
        {
            FindFolder(folderPath + filePath);

            batchDictionary.Clear();
            oldIndex = 0;
            GameObject[] go = Selection.gameObjects;
            for (int i = 0; i < go.Length; i++)
            {
                //GameObject go = this.gameObject;
                MeshFilter[] meshFilter = go[i].GetComponentsInChildren<MeshFilter>();
                for (int f = 0; f < meshFilter.Length; f++)
                {
                    Mesh addMesh = meshFilter[f].sharedMesh;
                    if (addMesh == null)
                        continue;

                    oldIndex++;
                    Renderer addRenderer = meshFilter[f].GetComponent<Renderer>();
                    Material[] addMaterials = addRenderer.sharedMaterials;

                    string dictKey = StringBatchKey(addMesh.name + addMaterials[0].name);// ���͸������ ���� ������Ʈ ������
                                                                                         // ��ųʸ� ���
                    if (batchDictionary.ContainsKey(dictKey))
                    {
                        Batch addBatch = batchDictionary[dictKey];
                        Matrix4x4 matrix = GetMatrix(addRenderer.transform);
                        addBatch.AddMatrix(matrix);
                    }
                    else
                    {
                        // ���ο� ��ųʸ� ī�װ� �����
                        Batch addBatch = new()
                        {
                            name = addMesh.name,
                            mesh = addMesh,
                            materials = addMaterials
                        };
                        batchDictionary[dictKey] = addBatch;
                        Matrix4x4 matrix = GetMatrix(addRenderer.transform);
                        addBatch.AddMatrix(matrix);
                    }
                }
            }
            Combine();
        }

        string StringBatchKey(string _dictKey)
        {
            int index = 0;
            string temp = _dictKey + index;
            bool finding = true;
            while (finding)
            {
                if (batchDictionary.ContainsKey(temp) == true)
                {
                    if (batchDictionary[temp].matrix.Count < 1023)// 1023���� �ڸ���
                    {
                        finding = false;
                    }
                    else
                    {
                        index++;
                        temp = _dictKey + index;
                    }
                }
                else
                {
                    finding = false;
                }
            }
            return temp;
        }

        Matrix4x4 GetMatrix(Transform _object)
        {
            Vector3 pos = _object.position;  // ������Ʈ ��ġ
            Quaternion rot = _object.rotation;// ������Ʈ ȸ��
            Vector3 scale = _object.localScale;// ������Ʈ ������
            Transform parent = _object.parent;
            while (parent != null)// �θ� ������ ��� ��ġ��
            {
                scale = new Vector3(
                    scale.x * parent.localScale.x,
                    scale.y * parent.localScale.y,
                    scale.z * parent.localScale.z
                    );
                parent = parent.parent;
            }
            return Matrix4x4.TRS(pos, rot, scale);
        }
        //=======================================================================================================
        [System.Serializable]
        public class MeshMaterial
        {
            public int vertexCount;
            public List<Mesh> meshs;
            public Material[] materials;
            public List<Matrix4x4> matrix;

            public void SetMeshMaterial(Mesh _mesh, Material[] _materials, Matrix4x4 _matrix)
            {
                vertexCount += _mesh.vertexCount;

                if (meshs == null)
                    meshs = new List<Mesh>();
                meshs.Add(_mesh);
                materials = _materials;
                if (matrix == null)
                    matrix = new List<Matrix4x4>();
                matrix.Add(_matrix);
            }
        }

        void Combine()
        {
            Dictionary<string, MeshMaterial> combines = new Dictionary<string, MeshMaterial>();
            foreach (var child in batchDictionary)
            {
                Mesh mesh = child.Value.mesh;
                string key = StringMeshName(child.Value.materials[0].name, mesh.vertexCount, combines);
                for (int i = 0; i < child.Value.matrix.Count; i++)
                {
                    combines[key].SetMeshMaterial(mesh, child.Value.materials, child.Value.matrix[i]);
                }
            }

            newIndex = 0;
            foreach (var child in combines)
            {
                CombineInstance[] combine = new CombineInstance[child.Value.meshs.Count];
                for (int i = 0; i < child.Value.meshs.Count; i++)
                {
                    combine[i].mesh = child.Value.meshs[i];
                    combine[i].transform = child.Value.matrix[i];
                }

                Mesh tempMesh = new Mesh();
                tempMesh.name = $"{child.Key}";
                tempMesh.CombineMeshes(combine);

                Mesh newMesh = Mesh.Instantiate(tempMesh);
                SaveMesh(newMesh, tempMesh.name);

                GameObject inst = new GameObject($"[ {child.Key} ]");
                //inst.transform.SetParent(transform, true);

                MeshFilter meshFilter = inst.AddComponent<MeshFilter>();
                meshFilter.mesh = newMesh;
                MeshRenderer instRenderer = inst.AddComponent<MeshRenderer>();
                instRenderer.materials = child.Value.materials;
                inst.AddComponent<MeshCollider>();
                newIndex++;
            }
        }

        string StringMeshName(string _name, int _vecterCount, Dictionary<string, MeshMaterial> _combines)
        {
            int index = 0;
            string temp = _name + index;
            bool loop = true;
            while (loop == true)
            {
                if (_combines.ContainsKey(temp) == true)
                {
                    int vertexCount = _combines[temp].vertexCount;
                    if (vertexCount > 65536)// 8��Ʈ ���ý� �ִ� ����
                    {
                        index++;
                        temp = _name + index;
                    }
                    else
                    {
                        _combines[temp].vertexCount += _vecterCount;
                        loop = false;
                    }
                }
                else
                {
                    _combines[temp] = new MeshMaterial();
                    loop = false;
                }
            }
            return temp;
        }
#if UNITY_EDITOR
        void SaveMesh(Mesh _mesh, string _name)
        {
            string path = folderPath + filePath + $"/{_name}.asset";
            AssetDatabase.CreateAsset(_mesh, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.SaveAssets();
        }

        //======================================================================================
        // ���� ã��
        //======================================================================================

        static void FindFolder(string folderName)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderName);
            if (dirInfo.Exists == false)
            {
                // ������ �����
                dirInfo.Create();
            }
        }
#endif
    }

}