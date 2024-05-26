
print("Unity to JSON exporter...");

finalAllMetaFile = "tools/all_meta.json";

import json
def writeJsonToFile(obj, path):
    print("Writing file '" + path + "'...");
    with open(path, 'w') as f:
        json.dump(obj, f)

srcYamlPath = "UnityProject/VRBiofield/Assets/Scenes.meta";
import yaml
def jsonObjFromYamlPath(yamlPath):
    data = None;
    with open(yamlPath, 'r') as f:
        data = yaml.load(f, Loader=yaml.SafeLoader)
        #print("data=", data);
    return data;
#print(jsonObjFromYamlPath(srcYamlPath))

import io;
def fileExists(path):
    return os.path.isfile(path);

def jsonObjFromUnityFile(unityPath):
    # read all lines:
    lines = [];
    resultLines = [];
    with open(unityPath, 'r') as f:
        lines = f.readlines();
    
    # break it into objects:
    parts = [];
    activePart = None;
    indent = "";
    for ln in lines:
        modLn = ln;
        if (ln.startswith("%")): continue;
        if (ln.startswith("---")):
            adrStr = ln.rfind("&");
            assert(adrStr >= 0);
            id = ln[adrStr+1:].strip();
            activePart = [];
            item = { 'fileID':id, 'yaml':activePart }
            modLn = "fileID: " + id + "\n";
            indent = '  ';
            parts.append(item);
            continue;
        assert(activePart is not None);
        activePart.append(ln);
    
    # load the YAML for each object:
    byFileId = {};
    partCount = len(parts);
    partIndex = 0;
    for part in parts:
        textStream = io.StringIO();
        for line in part['yaml']:
            textStream.write(line);
        resultText = textStream.getvalue();
        print("ResultText=", resultText);
        print("Parsing...", partIndex, " of ", partCount);
        partIndex = partIndex + 1;
        data = yaml.load(resultText, Loader=yaml.SafeLoader)
        print("Resolved.");
        fileID = part['fileID'];
        assert(not(fileID in byFileId));
        byFileId[fileID] = data;
    return byFileId;

#print(jsonObjFromUnityFile("UnityProject/VRBiofield/Assets/ChiVR_MainApp_Chakras.unity"));



srcYamlDir = "UnityProject/VRBiofield/Assets/";
import os;
knownYamlExtensions = { ".meta" };
def filesInFolder(path):
    tree = os.walk(path)
    ans = [];
    for (root, dirs, files) in tree:
        for file in files:
            lastDot = file.rfind(".");
            if (lastDot < 0):
                assert(False);
                continue;
            ext = file[lastDot:];
            isMeta = (ext in knownYamlExtensions);
            item = { 'path':(root + "/" + file), 'is_meta':isMeta }
            ans.append(item);
    return ans;
allFiles = filesInFolder(srcYamlDir);

allYamlsByGuid = {};
allFilesWithoutGuid = [];
finalResult = {'by_guid':allYamlsByGuid, 'files':allFilesWithoutGuid };

def writeAllJsonToFile():
    writeJsonToFile(finalResult, finalAllMetaFile);

def buildAllJson():
    for fileInfo in allFiles:
        filePath = fileInfo['path'];
        item = { 'path':filePath };
        isMeta = fileInfo['is_meta'];
        if (isMeta):
            print("Meta path:", filePath);
            obj = jsonObjFromYamlPath(filePath);
            keyGuid = obj['guid'];
            if (not keyGuid):
                raise "No valid guid!";
            item['yaml'] = obj;
            item['guid'] = keyGuid;
            assert(not (keyGuid in allYamlsByGuid));
            allYamlsByGuid[keyGuid] = item;
        else:
            allFilesWithoutGuid.append(item);
    writeAllJsonToFile();
    return allYamlsByGuid;

# check if main list exists, and create if not:
if (not fileExists(finalAllMetaFile)):
    buildAllJson();
    writeAllJsonToFile();


def ensureJsonFromUnity(unityPath):
    jsonPath = unityPath + ".json";
    if (fileExists(jsonPath)):
        return jsonPath;
    obj = jsonObjFromUnityFile(unityPath);
    writeJsonToFile(obj, jsonPath);
    return jsonPath;
ensureJsonFromUnity("UnityProject/VRBiofield/Assets/ChiVR_MainApp_Chakras.unity");

print("Done.");


