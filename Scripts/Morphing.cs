using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Morphing : MonoBehaviour
{
    public Mesh StartObject;
    public Mesh EndObject;

    MeshFilter result;

    public bool disloc;

    private bool invalide = false;
    
    // mesh tempon
    private Mesh m_mesh;
    // données du mesh de depart
    private Vector3[] m_vertices_Start;
    private int[] m_triangle_Start;

    // données du mesh à modifier
    private Vector3[] m_vertices_Morph;
    private int[] m_triangles_Morph;

    // données du mesh de fin
    private Vector3[] m_vertices_End;
    private int[] m_triangle_End;

    // distances entre les sommets de départ et de fin
    private Vector3[] m_Distances;
    
    // variables de deroulement du morphing
    private bool stop = true;
    private int step = 0;
    private int decalage = 0;
    private float compteur = 0.01f;

    // variables de deroulement de la subdivision
    private List<Vector3> vertices;
    private List<Vector3> normales;
    private Dictionary<uint, int> newVectices;
    private List<int> indices;

    GameObject ImportPanel;

    public void OnTrigerdisloc(Toggle toggle)
    {
        disloc = toggle.isOn;
    }

    private void Start()
    {
        GameObject.Find("A").GetComponent<Text>().text = StartObject.name;
        GameObject.Find("B").GetComponent<Text>().text = EndObject.name;

        result = GetComponent<MeshFilter>();
        if (StartObject != null && EndObject != null)
        {
            m_vertices_Start = StartObject.vertices;
            m_triangle_Start = StartObject.triangles;

            m_vertices_End = EndObject.vertices;
            m_triangle_End = EndObject.triangles;

            // initialisation du mesh a modifier et des disatnces a parcourir
            m_vertices_Morph = new Vector3[m_triangle_End.Length];
            m_triangles_Morph = new int[m_triangle_End.Length];

            // initialisation de la distance a parcourir pour chaque sommet
            m_Distances = new Vector3[m_triangle_End.Length];
        }
        else
        {
            step = 10;
        }
    }

    public void OnClickStart()
    {
        GameObject.Find("InfoPanel").GetComponent<Text>().text = "Started";
        if (ImportPanel == null)
            ImportPanel = GameObject.Find("ImportPanel");
        ImportPanel.SetActive(false); 
        stop = false;
    }


    public void SelectObjectB()
    {
        EndObject = null;
        step = 10;
        made = true;
        GameObject loadedObj = Resources.Load("ObjectB") as GameObject;
        MeshFilter meshFilter = loadedObj.GetComponent<MeshFilter>();
        Mesh loadedMesh = meshFilter.sharedMesh;
        GameObject object1 = Instantiate(loadedObj) as GameObject;
        EndObject = loadedMesh;
    }

    public void SelectObjectA()
    {
        StartObject = null;
        step = 10;
        made = true;
        GameObject loadedObj = Resources.Load("ObjectA") as GameObject;
        MeshFilter meshFilter = loadedObj.GetComponent<MeshFilter>();
        Mesh loadedMesh = meshFilter.sharedMesh;
        GameObject object1 = Instantiate(loadedObj) as GameObject;
        StartObject = loadedMesh;
    }

    private void Update()
    {


        if (step != 10)
        {
            if (!stop)
            {
                GameObject.Find("InfoPanel").GetComponent<Text>().text = "Subdivision";
                if (step == 0)
                {
                    _Subdivide();
                    if (m_triangle_Start.Length == m_triangle_End.Length)
                    {
                        // etape suivante
                        step = 1;
                    }
                }
                else
                {
                    GameObject.Find("InfoPanel").GetComponent<Text>().text = "Morphing";
                    _Morphing();
                }
            }
            else
            {
                GameObject.Find("InfoPanel").GetComponent<Text>().text = "Waiting for start";
                if(ImportPanel == null)
                    ImportPanel = GameObject.Find("ImportPanel");
                ImportPanel.SetActive(true);
            }
        }
        else
        {
            if (StartObject != null && EndObject != null)
            {
                m_vertices_Start = StartObject.vertices;
                m_triangle_Start = StartObject.triangles;

                m_vertices_End = EndObject.vertices;
                m_triangle_End = EndObject.triangles;

                // initialisation du mesh a modifier et des disatnces a parcourir
                m_vertices_Morph = new Vector3[m_triangle_End.Length];
                m_triangles_Morph = new int[m_triangle_End.Length];

                // initialisation de la distance a parcourir pour chaque sommet
                m_Distances = new Vector3[m_triangle_End.Length];

                step = 0;
            }
        }
    }

    //###############################
    //          Morphing
    //###############################

    private void _Morphing()
    {
        if (disloc)
        {
            if (step == 3)
            {
                string str_NewName = "Morph_" + EndObject.name;
                StartObject.name = str_NewName;
                step = 0;
                compteur = 0;
                stop = true;
            }
            else
            {
                for (int ind = 0; ind < m_triangles_Morph.Length; ind += 3)
                {
                    // vertices et triangles du mesh a modifier
                    m_vertices_Morph[ind] = m_vertices_Start[m_triangle_Start[ind]];
                    m_triangles_Morph[ind] = ind;
                    m_vertices_Morph[ind + 1] = m_vertices_Start[m_triangle_Start[ind + 1]];
                    m_triangles_Morph[ind + 1] = ind + 1;
                    m_vertices_Morph[ind + 2] = m_vertices_Start[m_triangle_Start[ind + 2]];
                    m_triangles_Morph[ind + 2] = ind + 2;

                    // distance pour chaque sommet
                    m_Distances[ind] = m_vertices_End[m_triangle_End[ind]] - m_vertices_Start[m_triangle_Start[ind]];
                    m_Distances[ind + 1] = m_vertices_End[m_triangle_End[ind + 1]] - m_vertices_Start[m_triangle_Start[ind + 1]];
                    m_Distances[ind + 2] = m_vertices_End[m_triangle_End[ind + 2]] - m_vertices_Start[m_triangle_Start[ind + 2]];

                }

                step = 2;
            }
            if (step == 2 && compteur < 1)
            {

                for (int ind = 0; ind < m_vertices_Morph.Length; ind++)
                {
                    m_vertices_Morph[ind] += (m_Distances[ind]) * compteur;
                }

                Mesh newMesh = new Mesh();

                newMesh.vertices = m_vertices_Morph;
                newMesh.triangles = m_triangles_Morph;
                newMesh.RecalculateNormals();

                result.mesh = newMesh;
                compteur += 0.005f;
            }
            else if (step == 2 && compteur >= 1)
            {
                step = 3;
            }
        }
        else
        {
            if (made) {
                for (int ind = 0; ind < m_triangles_Morph.Length; ind += 3)
                {
                    // vertices et triangles du mesh a modifier
                    m_vertices_Morph[ind] = m_vertices_Start[m_triangle_Start[ind]];
                    m_triangles_Morph[ind] = ind;
                    m_vertices_Morph[ind + 1] = m_vertices_Start[m_triangle_Start[ind + 1]];
                    m_triangles_Morph[ind + 1] = ind + 1;
                    m_vertices_Morph[ind + 2] = m_vertices_Start[m_triangle_Start[ind + 2]];
                    m_triangles_Morph[ind + 2] = ind + 2;
                }


                Mesh newMesh = new Mesh();

                newMesh.vertices = m_vertices_Morph;
                newMesh.triangles = m_triangles_Morph;
                newMesh.RecalculateNormals();

                Obj_A = newMesh;
                Obj_B = EndObject;
                string str_NewName = "Morph_" + EndObject.name;
                StartObject.name = str_NewName;
                step = 0;
                compteur = 0;
                stop = true;
                made = false;



                    scan_a();
                scan_b(); //on tri les deux mesh par points haut > bas 

                mesh = this.GetComponent<MeshFilter>().mesh;    //attribution du mesh au mesh de départ
                mesh.Clear();                                   //attribution du mesh au mesh de départ
                mesh.vertices = Obj_A.vertices;                 //attribution du mesh au mesh de départ
                mesh.uv = Obj_A.uv;                             //attribution du mesh au mesh de départ
                mesh.triangles = Obj_A.triangles;               //attribution du mesh au mesh de départ

                MorphVertices = new Vector3[Obj_A.vertices.Length];     //initialisation des tableaux   
                MorphTris = new int[Obj_A.triangles.Length];            //initialisation des tableaux
                MorphDeltaVertices = new Vector3[Obj_A.vertices.Length];//initialisation des tableaux

                print("le nombre de vertices de a : " + Obj_A.vertices.Length);
                print("le nombre de vertices de b : " + Obj_B.vertices.Length);
                nbEssais = 0;
                testAlgo();
                MorphStartToEnd();
            }
            MorphStartToEnd();
        }
    }

    bool made = true;
    //###############################
    //          Subdivision
    //###############################
    private void _Subdivide()
    {

        if (m_triangle_Start.Length < m_triangle_End.Length)
        {
            // si la taille de m_triangle_Start * 4 < la taille de m_triangle_End alors on divise tout les triangles de base en 4
            while (m_triangle_Start.Length * 4 <= m_triangle_End.Length)
            {
                SubdivideIn4(StartObject);
                m_triangle_Start = StartObject.triangles;
                m_vertices_Start = StartObject.vertices;
            }

            // si la taille de m_triangle_Start < la taille de m_triangle_End alors on divise tout un triangle en 2 jusqu'a obtenir le nombre de triangles voulu en decalant le triangle a diviser
            while (m_triangle_Start.Length < m_triangle_End.Length)
            {
                SubdivideIn2(StartObject, decalage);
                m_triangle_Start = StartObject.triangles;
                m_vertices_Start = StartObject.vertices;

                decalage += 6;
            }
        }

        if (m_triangle_End.Length < m_triangle_Start.Length)
        {
            // si la taille de m_triangle_End * 4 < la taille de m_triangle_Start alors on divise tout les triangles de base en 4
            while  (m_triangle_End.Length * 4 <= m_triangle_Start.Length)
            {
                SubdivideIn4(EndObject);
                m_triangle_End = EndObject.triangles;
                m_vertices_End = EndObject.vertices;
            }

            // si la taille de m_triangle_End < la taille de m_triangle_Start alors on divise tout un triangle en 2 jusqu'a obtenir le nombre de triangles voulu en decalant le triangle a diviser
            while (m_triangle_End.Length < m_triangle_Start.Length)
            {
                SubdivideIn2(EndObject, decalage);
                m_triangle_End = EndObject.triangles;
                m_vertices_End = EndObject.vertices;

                decalage += 6;
            }
        }
    }
    /// <summary>
    /// https://answers.unity.com/questions/259127/does-anyone-have-any-code-to-subdivide-a-mesh-and.html
    /// </summary>
    int GetNewVertex(int i1, int i2)
    {
        // We have to test both directions since the edge
        // could be reversed in another triangle
        uint t1 = ((uint)i1 << 16) | (uint)i2;
        uint t2 = ((uint)i2 << 16) | (uint)i1;
        if (newVectices.ContainsKey(t2))
            return newVectices[t2];
        if (newVectices.ContainsKey(t1))
            return newVectices[t1];
        // generate vertex:
        int newIndex = vertices.Count;
        newVectices.Add(t1, newIndex);

        // calculate new vertex
        vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);
        normales.Add((normales[i1] + normales[i2]).normalized);
        // [... all other vertex data arrays]

        return newIndex;
    }
    private void SubdivideIn4(Mesh mesh)
    {
        newVectices = new Dictionary<uint, int>();

        vertices = new List<Vector3>(mesh.vertices);
        normales = new List<Vector3>(mesh.normals);
        // [... all other vertex data arrays]
        indices = new List<int>();

        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            int a = GetNewVertex(i1, i2);
            int b = GetNewVertex(i2, i3);
            int c = GetNewVertex(i3, i1);
            indices.Add(i1); indices.Add(a); indices.Add(c);
            indices.Add(i2); indices.Add(b); indices.Add(a);
            indices.Add(i3); indices.Add(c); indices.Add(b);
            indices.Add(a); indices.Add(b); indices.Add(c); // center triangle
        }
        mesh.vertices = vertices.ToArray();
        mesh.normals = normales.ToArray();
        // [... all other vertex data arrays]
        mesh.triangles = indices.ToArray();

        // since this is a static function and it uses static variables
        // we should erase the arrays to free them:
        newVectices = null;
        vertices = null;
        normales = null;
        // [... all other vertex data arrays]

        indices = null;
    }
    private void SubdivideIn2(Mesh mesh, int decalage)
    {
        newVectices = new Dictionary<uint, int>();

        vertices = new List<Vector3>(mesh.vertices);
        normales = new List<Vector3>(mesh.normals);
        // [... all other vertex data arrays]
        indices = new List<int>();

        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (i == decalage)
            {
                int i1 = triangles[i + 0];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];

                int a = GetNewVertex(i1, i2);
                indices.Add(i1); indices.Add(a); indices.Add(i3);
                indices.Add(a); indices.Add(i2); indices.Add(i3);
            }
            else
            {
                int i1 = triangles[i + 0];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];

                indices.Add(i1); indices.Add(i2); indices.Add(i3);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normales.ToArray();
        // [... all other vertex data arrays]
        mesh.triangles = indices.ToArray();

        // since this is a static function and it uses static variables
        // we should erase the arrays to free them:
        newVectices = null;
        vertices = null;
        normales = null;
        // [... all other vertex data arrays]

        indices = null;
    }



    Mesh Obj_A;     //mesh de départ
    Mesh Obj_B;     //mesh d'arrivée

    Vector3[] MorphVertices; //tableaux pour modifier les vertices au fil du temps
    Vector3[] MorphDeltaVertices;

    //tableaux de booléens pour voir si le point à été attribué ou non
    List<bool> listeBool_a = new List<bool>();
    List<bool> listeBool_b = new List<bool>();

    //les tableaux qui vont contenir les points de coordonnées semblables de A et de B
    List<Vector3> ListeVertex_A = new List<Vector3>();
    List<Vector3> ListeVertex_B = new List<Vector3>();

    int[] MorphTris; //tableau de triangles

    float Point_haut_A;  //variable qui conctien le point pour le scan
    float Point_haut_B;  //variable qui conctien le point pour le scan
    int nbEssais;

    private Mesh mesh; //mesh de l'objet

    // Use this for initialization



    void scan_a()
    {
        List<Vector3> liste = new List<Vector3>();

        for (int i = 0; i < Obj_A.vertices.Length; ++i)
        {

            liste.Add(Obj_A.vertices[i]);

        }

        liste.Sort((a, b) => a.z.CompareTo(b.z));

        for (int i = 0; i < Obj_A.vertices.Length; ++i)
        {

            Obj_A.vertices[i] = liste[i];

        }


    }

    void scan_b()
    {
        List<Vector3> liste = new List<Vector3>();

        for (int i = 0; i < Obj_B.vertices.Length; ++i)
        {

            liste.Add(Obj_B.vertices[i]);

        }

        liste.Sort((a, b) => a.z.CompareTo(b.z));

        for (int i = 0; i < Obj_B.vertices.Length; ++i)
        {

            Obj_B.vertices[i] = liste[i];

        }
    }

    // private void InitMeshObjStart()
    // {
    //     //Doit parcourir tous les vertexs de la forme un, comparer avec la position de ceux de la forme deux ( trouver voisins ... ), et enfin mettre le vertex dans la liste tampon
    //
    //     for (int i = 0; i < Obj_A.vertices.Length; ++i)
    //     {
    //         MorphVertices[i] = Obj_A.vertices[i];
    //         MorphDeltaVertices[i] = (Obj_B.vertices[i] - Obj_A.vertices[i]) / 600.0f; //on attribue le point 1 de a au point 1 de b et vice versa
    //     }
    //
    //     for (int i = 0; i < Obj_A.triangles.Length; ++i)
    //         MorphTris[i] = Obj_A.triangles[i];  //same pour les triangles
    //
    //     mesh.vertices = MorphVertices;   //on change le mesh
    //     mesh.triangles = MorphTris;      //on change le mesh
    //     mesh.RecalculateNormals();       //on change le mesh
    //
    //
    // }

    void MorphStartToEnd() //modification des points dans le temps
    {
        if (nbEssais < 600)
        {
            nbEssais++;
            for (int i = 0; i < Obj_A.vertices.Length; ++i)
            {
                MorphVertices[i] += MorphDeltaVertices[i];
            }
            result.mesh.vertices = MorphVertices;
        }
    }


    void testAlgo()
    {
        //les tableaux qui vont contenir les points de coordonnées semblables de A et de B
        List<bool> list_bool_a = new List<bool>();
        List<bool> list_bool_b = new List<bool>();
        List<bool> list_bool_a_tri = new List<bool>();
        List<bool> list_bool_b_tri = new List<bool>();

        for (int b = 0; b < Obj_A.vertices.Length; ++b)
        {

            listeBool_a.Add(false);

        }


        for (int b = 0; b < Obj_B.vertices.Length; ++b)
        {

            listeBool_b.Add(false);

        }

        //lance le scan de A & B, attribution des points dans les tableaux;
        // on a donc des listes de point les plus hauts de A et de B

        int i = 0; // compteur pour A
        int j = 0; //compteur pour B
        //atnt que tous les points ne sont pas attribués

        if (Obj_A.vertices.Length >= Obj_B.vertices.Length)
        {
            while (i < Obj_B.vertices.Length)
            {
                listeBool_a[i] = true;  //on dit que les points sont attribués
                listeBool_b[i] = true;
                MorphVertices[i] = Obj_A.vertices[i];
                MorphDeltaVertices[i] = (Obj_B.vertices[i] - Obj_A.vertices[i]) / 600.0f; //on attribue le point 1 de a au point 1 de b et vice versa
                ++i; // on incrémente 
                result.mesh.vertices = MorphVertices;
            }

            if (!Isallattribued("A"))
            {
                j = i;
                while (j < Obj_A.vertices.Length)
                {
                    MorphVertices[j] = Obj_A.vertices[j];
                    MorphDeltaVertices[i] = (Obj_B.vertices[i] - Obj_A.vertices[j]) / 600.0f; //on attribue le point 1 de a au point 1 de b et vice versa*
                    result.mesh.vertices = MorphVertices;
                    j++;
                }
            }

            int[] Morph_triangles = new int[Obj_B.triangles.Length];   ///attribution des traingles

            for (int t = 0; t < Obj_B.triangles.Length; ++t)
            {
                Morph_triangles[t] = Obj_B.triangles[t];
            }

            result.mesh.triangles = Morph_triangles;


            //mesh.triangles = MorphTris;

        }

        else
        {
            while (i < Obj_A.vertices.Length)
            {
                Obj_A.vertices[i] = Obj_B.vertices[i]; // on change le mesh a en le mesh b 
                listeBool_a[i] = true;  //on dit que les points sont attribués
                listeBool_b[i] = true;
                MorphVertices[i] = Obj_A.vertices[i];
                MorphDeltaVertices[i] = (Obj_B.vertices[i] - Obj_A.vertices[i]) / 600.0f; //on attribue le point 1 de a au point 1 de b et vice versa
                ++i; // on incrémente 

                result.mesh.vertices = MorphVertices;

            }

            if (!Isallattribued("B"))
            {
                j = i;
                while (j < Obj_B.vertices.Length)
                {
                    MorphVertices[j] = Obj_B.vertices[j];
                    MorphDeltaVertices[i] = (Obj_A.vertices[i] - Obj_B.vertices[j]) / 600.0f; //on attribue le point 1 de a au point 1 de b et vice versa
                    result.mesh.vertices = MorphVertices;
                    j++;
                }
            }


        }

    }


    bool Isallattribued(string lettre)
    {
        if (lettre == "A")
        {
            for (int i = 0; i < Obj_A.vertices.Length; ++i)
            {
                if (listeBool_a[i] == false)
                    return false;
            }
            return true;
        }

        else if (lettre == "B")
        {
            for (int i = 0; i < Obj_B.vertices.Length; ++i)
            {
                if (listeBool_b[i] == false)
                    return false;
            }
            return true;
        }

        return false;
    }

}
