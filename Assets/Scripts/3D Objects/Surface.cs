using System.IO;                    //Stream/BinaryReader
using UnityEngine;
using System.Collections;
using System.Collections.Generic;   //List<T>

public class Surface
{
    public List<Vector3> verticesObj = new List<Vector3>();          //List of X-Y-Z Coordinates : each element is one vetice (point)
    public int[] idTri;                                              //List of Index number : three vertices used to make one triangle

    public Surface(string triFilePath)
    {
        readTriFile(triFilePath);
    }

    void readTriFile(string filePath)
    {
        string lineStream = "";
        int numberVertices = 0;
        int numberTriangles = 0;
        Vector3 vertice = new Vector3(0, 0, 0);

        using (StreamReader sr = new StreamReader(filePath))
        {
            /*=== Vertices Extraction ===*/
            lineStream = sr.ReadLine().Split(new char[] { '-' }).GetValue(1).ToString();
            numberVertices = int.Parse(lineStream);

            for (int i = 0; i < numberVertices; i++)
            {
                lineStream = sr.ReadLine();
                vertice.x = float.Parse(lineStream.Split(new char[] { ' ' }).GetValue(0).ToString());
                vertice.y = float.Parse(lineStream.Split(new char[] { ' ' }).GetValue(1).ToString());
                vertice.z = float.Parse(lineStream.Split(new char[] { ' ' }).GetValue(2).ToString());

                /********************************************************************************/
                /* /!\ Transformation à appliquer pour que ca soit vraiment un maillage MNI /!\ */
                /*       => Remerciement à la conversion de intranat faite avec les pieds       */
                /********************************************************************************/
                vertice = new Vector3(-0.999f * vertice.x + 0 * vertice.y + 0 * vertice.z + 74.04f,
                0 * vertice.x + -0.999f * vertice.y + 0 * vertice.z + 76.599f,
                0 * vertice.x + 0 * vertice.y + -0.999f * vertice.z + 87.459f);
                /************************* /!\Axe x de unity inversé /!\ ************************/
                vertice.x = -vertice.x;
                /********************************************************************************/
                verticesObj.Add(vertice);
            }

            /*=== Triangles Extraction ===*/
            lineStream = sr.ReadLine().Split(new char[] { '-', ' ' }).GetValue(2).ToString();
            numberTriangles = int.Parse(lineStream);

            idTri = new int[3 * numberTriangles];
            for (int i = 0; i < numberTriangles; ++i)
            {
                lineStream = sr.ReadLine();
                idTri[(3 * i)] = int.Parse(lineStream.Split(new char[] { ' ' }).GetValue(0).ToString());
                idTri[(3 * i) + 1] = int.Parse(lineStream.Split(new char[] { ' ' }).GetValue(1).ToString());
                idTri[(3 * i) + 2] = int.Parse(lineStream.Split(new char[] { ' ' }).GetValue(2).ToString());
            }
            sr.Close();
        }
    }
}
