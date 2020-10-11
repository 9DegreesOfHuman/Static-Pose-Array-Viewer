using System;
using System.IO;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;

public class DebugRenderer : MonoBehaviour
{
    Skeleton skeleton;
    GameObject[] blockman;
    public GameObject blockmanParent;
    // public GameObject blockmanParentRotateAxis;
    // public Renderer renderer;
    public string folderPath = @"D:\Downloads\squat-front-100-dan-csv";
    string[] filePaths;
    int currentFileIndex = 0;
    int fileIndexToLoad = 0;
    string[] _currentFile;
    int currentSkeletonIndex = 1;
    int skeletonIndexToLoad = 1;
    string[] currentPoseCoords;
    string[] poseLabels;
    public Text text_currentSkeletonIndex;
    public Text text_currentFileIndex;
    public InputField skeletonIndexInput;
    public InputField filePathInput;
    public InputField markAs1;
    public InputField markAs2;
    public InputField markAs3;
    public InputField markAs4;
    int numJoints = 0;

    void Start(){
        Debug.Log("start");

        numJoints = (int)JointId.Count;
        if (numJoints < 32) numJoints = 32;

        blockman = makeBlockman();
        filePaths = getPoseFiles();

        // correctFiles(); // use if the file strings need any modification

        loadFile(currentFileIndex);
    }

    int correctFiles() {
        Debug.Log($"correcing {filePaths.Length} files");
        for (int i = 0; i < filePaths.Length; i++) {
            if (filePaths == null || filePaths.Length < 1) {
                Debug.Log("filePaths array is null or empty");
                return -1;
            }
            if (i >= filePaths.Length) {
                Debug.Log("reached end of file list");
                return -1;
            }

            Debug.Log("filePaths array is ok");

            string currentFilePath = filePaths[i];
            string[] thisFile = System.IO.File.ReadAllLines(@currentFilePath);
            if (thisFile == null || thisFile.Length < 2) { // header + data row 
                Debug.Log("thisFile array is null or empty");
                return -1;
            }

            for (int j = 0; j < filePaths.Length; j++) {
                thisFile[j] = thisFile[j].Replace("/", ",");
            }
            Debug.Log("replaced files");

            string[] folderParts = currentFilePath.Split('\\');                                          // ["D:", "Downloads", "squat-front-100-dan-csv"]
            Debug.Log($"folderParts: {string.Join("^", folderParts)}");
            // folderParts: D: ^path ^ skeletalData ^ butterfly - front.txt

            string folder = folderParts[folderParts.Length - 2] + @"\";                             //                     "squat-front-100-dan-csv\"
            Debug.Log($"folder: {folder}");

            string currFilePath = filePaths[i];                                      // "D:\Downloads\squat-front-100-dan-csv\6112020_101909-PM.txt"
            Debug.Log($"currFilePath: {currFilePath}");
            string[] parts = currFilePath.Split(new string[] { folder }, StringSplitOptions.None);  // ["D:\Downloads\",                                  "6112020_101909-PM.txt"]
            Debug.Log(parts.Length);
            Debug.Log(parts[0]);
            Debug.Log(parts[1]);
            string fileExt = parts[1].Substring(0, parts[1].Length - 3) + ".csv";
            string newFilePath = parts[0] + folder + @"labelledPoses\" + parts[1];                  // "D:\Downloads\squat-front-100-dan-csv\labelledPoses\6112020_101909-PM.txt"
            Debug.Log("Writing to " + newFilePath);
            string data = "";
            for (int k = 0; k < thisFile.Length; k++)
            {
                data = data + thisFile[i] + "\n";
            }

            bool retValue = false;
            string dataPath = parts[0] + folder + @"labelledPoses\";
            string fileName = parts[1];
            try
            {
                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }
                dataPath += fileName;

                System.IO.File.WriteAllText(dataPath, data);
                retValue = true;
            }
            catch (System.Exception ex)
            {
                string ErrorMessages = "File Write Error\n" + ex.Message;
                retValue = false;
                Debug.LogError(ErrorMessages);
            }
            Debug.Log("done: " + retValue);
        }
        Debug.Log("done with all files");
        return 0;
    }

    int loadFile(int fileIndex){
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
        Debug.Log("Loading " + currentFilePath);
        _currentFile = System.IO.File.ReadAllLines(@currentFilePath);
        poseLabels = new string[_currentFile.Length];
        // Debug.Log(lines[0]); // headers
        
        if (_currentFile == null || _currentFile.Length < 2) // header + data row
        {
            Debug.Log("currentFile array is null or empty");
            return -1;
        }

        text_currentFileIndex.text = currentFilePath;

        currentPoseCoords = _currentFile[1].Split(',');
        currentFileIndex = fileIndex;
        Debug.Log("Loaded @ index " + fileIndex);
        return 0;
    }
    
    public void loadSkeletonFromInput() {
        string value = skeletonIndexInput.text;
        if(string.IsNullOrEmpty(value)) Debug.LogError("Skeleton index is empty");
        
        if (Int32.TryParse(value, out int numValue)) {
            skeletonIndexToLoad = numValue;
            // loadSkeleton(numValue);
            return;
        }
        
        Debug.Log($"Int32.TryParse could not parse '{value}' to an int.");
    }
    int loadSkeleton(int idxToLoad){
        if (_currentFile == null || _currentFile.Length < 2) // header + data row
        {
            Debug.Log("currentFile array is null or empty");
            return -1;
        }
        
        if (idxToLoad < _currentFile.Length)
        {
            currentPoseCoords = _currentFile[idxToLoad].Split(',');
            return 0;
        }

        skeletonIndexToLoad = 0;
        writePoseArrayToFile();
        loadNextFile();

        return 0;
    }


    public void loadNextFile(){
        if(fileIndexToLoad < filePaths.Length) {
            fileIndexToLoad = currentFileIndex + 1;
            loadFile(fileIndexToLoad);
            Debug.Log("loadNextSkeleton");
        } else {
            Debug.Log("loadNextSkeleton reached limit");
        }
    }

    public void loadPrevFile(){
        if(fileIndexToLoad > 0) {
            fileIndexToLoad = currentFileIndex - 1;
            loadFile(fileIndexToLoad);
            Debug.Log("loadPrevSkeleton");
        } else {
            Debug.Log("loadPrevSkeleton reached limit");
        }
    }

    void Update(){
        if (currentSkeletonIndex != skeletonIndexToLoad)
        {
            loadSkeleton(skeletonIndexToLoad);
            currentSkeletonIndex = skeletonIndexToLoad;
        }
        text_currentSkeletonIndex.text = "current: " + currentSkeletonIndex + " - total: " + _currentFile.Length;
        updateBlockman(currentPoseCoords);

        if(Input.GetKeyDown(KeyCode.Keypad1)) { markAs_onClick1(); }
        if(Input.GetKeyDown(KeyCode.Keypad2)) { markAs_onClick2(); }
        if(Input.GetKeyDown(KeyCode.Keypad3)) { markAs_onClick3(); }
        if(Input.GetKeyDown(KeyCode.Keypad4)) { markAs_onClick4(); }
    }

    public void loadNextSkeleton(){
        if(skeletonIndexToLoad < _currentFile.Length) {
            skeletonIndexToLoad = currentSkeletonIndex + 1;
            Debug.Log("loadNextSkeleton");
        } else {
            Debug.Log("loadNextSkeleton reached limit");
        }
    }

    public void loadPrevSkeleton(){
        if(skeletonIndexToLoad > 0) {
            skeletonIndexToLoad = currentSkeletonIndex - 1;
            Debug.Log("loadPrevSkeleton");
        } else {
            Debug.Log("loadPrevSkeleton reached limit");
        }
    }

    private string[] labelCurrentSkeleton(string[] labels, int index, string label) {
        string[] labelsCopy = (string[]) labels.Clone(); // shallow copy
        if (labelsCopy != null && index < labelsCopy.Length) {
            labelsCopy[index] = label;
            Debug.Log("markAs_" + label);
            loadNextSkeleton();
        } else {
            Debug.Log("markAs_" + label + " index error");
        }
        return labelsCopy;
    }

    public void markAs_onClick1(){ // Debug.Log("markAs_onClick1: " + markAs1.text);
        poseLabels = labelCurrentSkeleton(poseLabels, currentSkeletonIndex, "," + markAs1.text + ",");
    }

    public void markAs_onClick2(){ // Debug.Log("markAs_onClick2: " + markAs2.text);
        poseLabels = labelCurrentSkeleton(poseLabels, currentSkeletonIndex, "," + markAs2.text + ",");
    }

    public void markAs_onClick3(){ // Debug.Log("markAs_onClick3: " + markAs3.text);
        poseLabels = labelCurrentSkeleton(poseLabels, currentSkeletonIndex, "," + markAs3.text + ",");
    }

    public void markAs_onClick4(){ // Debug.Log("markAs_onClick4: " + markAs4.text);
        poseLabels = labelCurrentSkeleton(poseLabels, currentSkeletonIndex, "," + markAs4.text + ",");
    }

    public void writePoseArrayToFile(){
        string[] folderParts = folderPath.Split('\\');                                          // ["D:", "Downloads", "squat-front-100-dan-csv"]
        string folder = folderParts[folderParts.Length - 1] + @"\";                             //                     "squat-front-100-dan-csv\"

        string currFilePath = filePaths[currentFileIndex];                                      // "D:\Downloads\squat-front-100-dan-csv\6112020_101909-PM.txt"
        string[] parts = currFilePath.Split(new string[] { folder }, StringSplitOptions.None);  // ["D:\Downloads\",                                  "6112020_101909-PM.txt"]
        string fileExt = parts[1].Substring(0,parts[1].Length-3) + ".csv";
        string newFilePath = parts[0] + folder + @"labelledPoses\" + parts[1];                  // "D:\Downloads\squat-front-100-dan-csv\labelledPoses\6112020_101909-PM.txt"
        Debug.Log("Writing to " + newFilePath);
        string data = "";
        for(int i=0;i<_currentFile.Length;i++) {
            data = data + _currentFile[i] + poseLabels[i] + "\n";
        }
        
         bool retValue = false;
         string dataPath = parts[0] + folder + @"labelledPoses\";
         string fileName = parts[1];
         try {
             if (!Directory.Exists (dataPath)) {
                 Directory.CreateDirectory (dataPath);
             }
             dataPath += fileName;

             System.IO.File.WriteAllText (dataPath, data);
             retValue = true;
         } catch (System.Exception ex) {
             string ErrorMessages = "File Write Error\n" + ex.Message;
             retValue = false;
             Debug.LogError (ErrorMessages);
         }
        Debug.Log("done: " + retValue);
    }

    void updateBlockman(string[] skeletonString){
        // Debug.Log(string.Join(",", skeletonString));

        int sizeOfJointArray = 5;
        // Joint,Pelvis,-18.73407,-125.5482,1376.917,
        for (var i = 0; i < numJoints; i++)
        {
            var jointSeparatorStr = skeletonString[i * sizeOfJointArray + 0];
            var jointNameStr = skeletonString[i * sizeOfJointArray + 1];
            var xStr = skeletonString[i * sizeOfJointArray + 2];
            var yStr = skeletonString[i * sizeOfJointArray + 3];
            var zStr = skeletonString[i * sizeOfJointArray + 4];

            // Debug.Log("\n" + "jointSeparatorStr:" + jointSeparatorStr);
            // Debug.Log("jointNameStr:" + jointNameStr);

            // Debug.Log("xStr:" + xStr);
            var x = float.Parse(xStr);

            // Debug.Log("yStr:" + yStr);
            var y = float.Parse(yStr);

            // Debug.Log("zStr:" + zStr + "\n");
            var z = float.Parse(zStr);

            var v = new Vector3(x, -y, z) * 0.004f;
            blockman[i].transform.localPosition = v;
        }
        //loadNextSkeleton();
    }

    string[] getPoseFiles(){
        Debug.Log("get files");
        DirectoryInfo d = new DirectoryInfo(folderPath);
        FileInfo[] Files = d.GetFiles("*.txt");
        
        string[] filePaths = new string[Files.Length];
        for (int i=0;i<Files.Length;i++)
        {
            filePaths[i] = Files[i].FullName;
            // Debug.Log("loaded " + filePaths[i]);
        }
        
        return filePaths;
    }

    GameObject[] makeBlockman(){
        GameObject[] debugObjects = new GameObject[numJoints];
        for (var i = 0; i < numJoints; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = Enum.GetName(typeof(JointId), i);
            cube.transform.localScale = Vector3.one * 0.4f;
            cube.transform.parent = blockmanParent.transform;
            debugObjects[i] = cube;
        }
        return debugObjects;
    }

}
