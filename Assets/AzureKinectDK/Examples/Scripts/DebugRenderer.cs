using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;

public class DebugRenderer : MonoBehaviour
{
    Skeleton skeleton;
    GameObject[] blockman;
    public Renderer renderer;
    string[] filePaths;
    int currentFileIndex = 0;
    string[] _currentFile;
    int currentSkeletonIndex = 1;
    string[] currentPoseCoords;
    Boolean advanceSkeleton = false;

    void Start()
    {
        Debug.Log("start");
        blockman = makeBlockman();
        filePaths = getPoseFiles();
        loadFile(currentFileIndex);
    }

    int loadFile(int fileIndex)
    {
        if (filePaths == null || filePaths.Length < 1)
        {
            Debug.Log("filePaths array is null or empty");
            return -1;
        }
        if (fileIndex >= filePaths.Length)
        {
            Debug.Log("reached end of file list");
            return -1;
        }

        string currentFilePath = filePaths[fileIndex];
        Debug.Log(currentFilePath);
        _currentFile = System.IO.File.ReadAllLines(@currentFilePath);
        // Debug.Log(lines[0]); // headers
        
        if (_currentFile == null || _currentFile.Length < 2) // header + data row
        {
            Debug.Log("currentFile array is null or empty");
            return -1;
        }

        currentPoseCoords = _currentFile[1].Split(',');

        return 0;
    }
    int loadNextSkeleton(int currIdx)
    {
        int nextIdx = currIdx + 1;

        if (_currentFile == null || _currentFile.Length < 2) // header + data row
        {
            Debug.Log("currentFile array is null or empty");
            return -1;
        }

        if (nextIdx < _currentFile.Length)
        {
            currentPoseCoords = _currentFile[nextIdx].Split(',');
            currentSkeletonIndex = nextIdx;
            return 0;
        }

        if (filePaths == null || filePaths.Length < 2) // header + data row
        {
            Debug.Log("filePaths array is null or empty");
            return -1;
        }

        Debug.Log("load next file");

        return 0;
    }

    void Update()
    {
        if (advanceSkeleton)
        {
            loadNextSkeleton(currentSkeletonIndex);
            advanceSkeleton = false;
        }
        updateBlockman(currentPoseCoords);
    }

    public void updateSkeleton()
    {
        advanceSkeleton = true;
        Debug.Log("update skeleton");
    }

    void updateBlockman(string[] skeletonString)
    {
        for (var i = 0; i < (int)JointId.Count; i++)
        {
            var x = float.Parse(skeletonString[i*3 + 0]);
            var y = float.Parse(skeletonString[i*3 + 1]);
            var z = float.Parse(skeletonString[i*3 + 2]);

            var v = new Vector3(x, -y, z) * 0.004f;
            blockman[i].transform.position = v;
        }
    }

    string[] getPoseFiles()
    {
        Debug.Log("get files");
        DirectoryInfo d = new DirectoryInfo(@"D:\Downloads\squat-front-100-dan-csv");
        FileInfo[] Files = d.GetFiles("*.txt");
        // Debug.Log(Files[0].Directory); // D:\Downloads\squat-front-100-dan-csv instance of folder
        // Debug.Log(Files[0].DirectoryName); // D:\Downloads\squat-front-100-dan-csv string
        // Debug.Log(Files[0].Name); // 6112020 101909 PM.txt file name
        // Debug.Log(Files[0].FullName); // D:\Downloads\squat-front-100-dan-csv\6112020 101909 PM.txt full path of dir or file

        string[] filePaths = new string[Files.Length];
        for (int i=0;i<Files.Length;i++) // foreach(FileInfo file in Files )
        {
            filePaths[i] = Files[i].FullName;
        }
        
        return filePaths;
    }

    GameObject[] makeBlockman()
    {
        GameObject[] debugObjects = new GameObject[(int)JointId.Count];
        for (var i = 0; i < (int)JointId.Count; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = Enum.GetName(typeof(JointId), i);
            cube.transform.localScale = Vector3.one * 0.4f;
            debugObjects[i] = cube;
        }
        return debugObjects;
    }
}
