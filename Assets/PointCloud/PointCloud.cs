
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PointCloud : MonoBehaviour
{
    ParticleSystem particle_system;
    ParticleSystem.Particle[] point_cloud;
    bool point_cloud_updated = false;

    public float point_scale = 0.07f;
    public float point_cloud_scale = 0.2f;

    void OnEnable()
    {
        particle_system = GetComponent<ParticleSystem>();
        var ps_emission = particle_system.emission;
        ps_emission.enabled = false;
        var ps_shape = particle_system.shape;
        ps_shape.enabled = false;
        var ps_main = particle_system.main;
        ps_main.playOnAwake = false;
        ps_main.maxParticles = 100000;
        ps_main.startSpeed = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> positions = new List<Vector3>();
        List<Color> colors = new List<Color>();

        string point_cloud_ply_filepath = "Assets/PointCloud/flowers.ply"; 
        //string point_cloud_ply_filepath = "Assets/PointCloud/flowerPoints.ply";
        //string point_cloud_ply_filepath = "Assets/PointCloud/stairs.ply";
        (positions, colors) = read_ply(point_cloud_ply_filepath);

        //string point_cloud_xyzrgb_filepath = "Assets/PointCloud/xyzrgb.txt";
        //(positions, colors) = read_xyzrgb_file(point_cloud_xyzrgb_filepath);

        SetPointCloud(positions.ToArray(), colors.ToArray());

    }

    // Update is called once per frame
    void Update()
    {
        if(point_cloud_updated)
        {
            point_cloud_updated = false;
            particle_system.SetParticles(point_cloud, point_cloud.Length);
        }
    }

    public void SetPointCloud(Vector3[] positions, Color[] colors)
    {
        point_cloud = new ParticleSystem.Particle[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            point_cloud[i].position = positions[i] * point_cloud_scale;
            point_cloud[i].startColor = colors[i];
            point_cloud[i].startSize = point_scale;
        }

        Debug.Log("Point cloud set! Number of points: " + point_cloud.Length);
        point_cloud_updated = true;
    }

    public (List<Vector3> positions, List<Color> colors ) read_ply(string file_name)
    {
        // https://paulbourke.net/dataformats/ply/
        List<string> header = new List<string>();
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Color> colors = new List<Color>();
        using (FileStream stream = File.Open(file_name, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, false))
            {
                // Read header: PLY contains ASCII CR header.
                while(true)
                {
                    // Read line from header.
                    List<char> chars = new List<char>();
                    while (true)
                    {
                        char c = reader.ReadChar();
                        chars.Add(c);
                        if (c.ToString() == Environment.NewLine)
                        {
                            break;
                        } 
                    }
                    string str = new string(chars.ToArray());

                    // Store header string.
                    header.Add(str);

                    // PLY header ends with "end_header".
                    if (str.Contains("end_header")) 
                    {
                        break;
                    }
                }
                // Print header and Extract important header info.
                long n_points = 0;
                List<string> header_element_vertex_properties = new List<string>();
                bool started_vertex_properties = false;
                for (int i = 0; i < header.Count; i++)
                {
                    //Debug.Log(header[i]);

                    if (header[i].Contains("element face") || header[i].Contains("end_header"))
                    {
                        started_vertex_properties = false;
                    }

                    if (started_vertex_properties)
                    {
                        header_element_vertex_properties.Add(header[i]);
                    }

                    // Extract important header info.
                    if (header[i].Contains("element vertex"))
                    {
                        string[] str_tokens = header[i].Split(" ");
                        n_points = long.Parse(str_tokens[2]);
                        started_vertex_properties = true;
                    }
                    
                }
                Debug.Log("Total number of points in PLY file: " +  n_points.ToString());

                for (int i = 0; i < header_element_vertex_properties.Count; i++)
                {
                    Debug.Log(header_element_vertex_properties[i]);
                }

                // Read points (element vertex).
                float density = 50000.0f / n_points;
                for (long point_i = 0; point_i < n_points; point_i++)
                {
                    // For each point we have a line containing properties: xyz nxnxynz rgb (ignore others).
                    Color color = new Color(0.0f, 0.0f, 0.0f);
                    Vector3 point = new Vector3(0.0f, 0.0f, 0.0f);
                    Vector3 normal = new Vector3(0.0f, 0.0f, 0.0f);
                    for (int i = 0; i < header_element_vertex_properties.Count; i++)
                    {
                        string[] str_tokens = header_element_vertex_properties[i].Split(" ");

                        if (str_tokens[2].Trim().Equals("x"))
                        {
                            if (str_tokens[1].Trim().Equals("float"))
                            {
                                point[0] = reader.ReadSingle();
                                
                            }
                            else if (str_tokens[1].Trim().Equals("double"))
                            {
                                point[0] = Convert.ToSingle(reader.ReadDouble());
                            }
                            else
                            {
                                Debug.Log("ERR: x coord not float or double!");
                            }
                        }
                        else if (str_tokens[2].Trim().Equals("y"))
                        {
                            if (str_tokens[1].Trim().Equals("float"))
                            {
                                point[1] = reader.ReadSingle();
                            }
                            else if (str_tokens[1].Trim().Equals("double"))
                            {
                                point[1] = Convert.ToSingle(reader.ReadDouble());
                            }
                            else
                            {
                                Debug.Log("ERR: y coord not float or double!");
                            }
                        }
                        else if (str_tokens[2].Trim().Equals("z"))
                        {
                            if (str_tokens[1].Trim().Equals("float"))
                            {
                                point[2] = reader.ReadSingle();
                            }
                            else if (str_tokens[1].Trim().Equals("double"))
                            {
                                point[2] = Convert.ToSingle(reader.ReadDouble());
                            }
                            else
                            {
                                Debug.Log("ERR: z coord not float or double!");
                            }
                        }
                        else if (str_tokens[2].Trim().Equals("nx"))
                        {
                            if (str_tokens[1].Trim().Equals("float"))
                            {
                                normal[0] = reader.ReadSingle();
                            }
                            else if (str_tokens[1].Trim().Equals("double"))
                            {
                                normal[0] = Convert.ToSingle(reader.ReadDouble());
                            }
                            else
                            {
                                Debug.Log("ERR: nx coord not float or double!");
                            }
                        }
                        else if (str_tokens[2].Trim().Equals("ny"))
                        {
                            if (str_tokens[1].Trim().Equals("float"))
                            {
                                normal[1] = reader.ReadSingle();
                            }
                            else if (str_tokens[1].Trim().Equals("double"))
                            {
                                normal[1] = Convert.ToSingle(reader.ReadDouble());
                            }
                            else
                            {
                                Debug.Log("ERR: ny coord not float or double!");
                            }
                        }
                        else if (str_tokens[2].Trim().Equals("nz"))
                        {
                            if (str_tokens[1].Trim().Equals("float"))
                            {
                                normal[2] = reader.ReadSingle();
                            }
                            else if (str_tokens[1].Trim().Equals("double"))
                            {
                                normal[2] = Convert.ToSingle(reader.ReadDouble());
                            }
                            else
                            {
                                Debug.Log("ERR: nz coord not float or double!");
                            }
                        }
                        else if (str_tokens[2].Trim().Equals("red"))
                        {
                            if (str_tokens[1].Trim().Equals("uchar"))
                            {
                                int r = reader.ReadByte();
                                color[0] = (float) r / 255.0f;
                            }
                            else
                            {
                                Debug.Log("ERR: red coord not uchar!");
                            }
                        }
                        else if (str_tokens[2].Trim().Equals("green"))
                        {
                            if (str_tokens[1].Trim().Equals("uchar"))
                            {
                                int r = reader.ReadByte();
                                color[1] = (float) r / 255.0f;
                            }
                            else
                            {
                                Debug.Log("ERR: red coord not uchar!");
                            }
                        }
                        else if (str_tokens[2].Trim().Equals("blue"))
                        {
                            if (str_tokens[1].Trim().Equals("uchar"))
                            {
                                int r = reader.ReadByte();
                                color[2] = (float) r / 255.0f;
                            }
                            else
                            {
                                Debug.Log("ERR: red coord not uchar!");
                            }
                        }
                        else
                        {
                            // Ignore others.
                            if (str_tokens[1].Trim().Equals("char"))
                            {
                                reader.ReadByte();
                            }
                            else if (str_tokens[1].Trim().Equals("uchar"))
                            {
                                reader.ReadByte();
                            }
                            else if (str_tokens[1].Trim().Equals("short"))
                            {
                                reader.ReadByte();
                                reader.ReadByte();
                            }
                            else if (str_tokens[1].Trim().Equals("int"))
                            {
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                            }
                            else if (str_tokens[1].Trim().Equals("uint"))
                            {
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                            }
                            else if (str_tokens[1].Trim().Equals("float"))
                            {
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                            }
                            else if (str_tokens[1].Trim().Equals("double"))
                            {
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                            }
                        }
                    }

                    // Not all points are stored for large point clouds.
                    if (UnityEngine.Random.Range(0.0f, 1.0f) < density)
                    {
                        //Debug.Log(color);
                        colors.Add(color);
                        //Debug.Log(normal);
                        normals.Add(normal);
                        //Debug.Log(point);
                        positions.Add(point);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
        return (positions, colors);
    }

    public (List<Vector3> positions, List<Color> colors ) read_xyzrgb_file(string xyz_rgb_filepath)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Color> colors = new List<Color>();

        using (StreamReader sr = new StreamReader(xyz_rgb_filepath))
        {
            while(!sr.EndOfStream)
            {
                string[] line_tokens = sr.ReadLine().Split(" ");
                positions.Add(new Vector3(float.Parse(line_tokens[0]), float.Parse(line_tokens[1]), float.Parse(line_tokens[2])));
                //colors.Add(new Color(float.Parse(line_tokens[3])/255.0f, float.Parse(line_tokens[4])/255.0f, float.Parse(line_tokens[5]))/255.0f);
                colors.Add(new Color(1.0f, 1.0f, 1.0f));
            }
        }

        return (positions, colors);
    }
}
