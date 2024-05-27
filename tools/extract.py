
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

def sceneThreeFromJsonScene(component_by_file_id, object_by_guid):
    by_types = {};
    root_transform = None;
    by_file_id = component_by_file_id;

    def typeNameAndObject(typedObj):
        first_key = list(typedObj.keys())[0];
        first_type = first_key;
        first_val = typedObj[first_key];
        return (first_type,first_val);

    for (index,file_id) in enumerate(component_by_file_id):
        file_obj = component_by_file_id[file_id];
        first_key = list(file_obj.keys())[0];
        first_type = first_key;
        first_val = file_obj[first_key];
        if (not(first_type in by_types)):
            by_types[first_type] = [];
        by_types[first_type].append(first_val);
    
    def getFileId(file_id):
        if (isinstance(file_id,dict) and 'fileID' in file_id):
            file_id = file_id['fileID'];
        id = str( file_id );
        if (id == "0"):
            return None;
        with_type = by_file_id[id];
        (type_name,obj) = typeNameAndObject(with_type);
        return obj;
    def getPtr(obj,prop):
        return getFileId(obj[prop]);
    def listFromDictVector(vecDict):
        ans = [];
        for k in vecDict:
            v = vecDict[k];
            ans.append(v);
        return ans;
    
    transTypeName = "Transform";
    assert(transTypeName in by_types);
    all_transforms = by_types[transTypeName];
    result_scenes = [];
    for transform in all_transforms:
        transform['out_index'] = len(result_scenes);
        gameObj = getPtr(transform,'m_GameObject');
        scene = { 
            'name':gameObj['m_Name'],
            'children':[],
            "position":listFromDictVector(transform['m_LocalPosition']),
            "quaternion":listFromDictVector(transform['m_LocalRotation']),
            "scale":listFromDictVector(transform['m_LocalScale']),
        };
        result_scenes.append(scene);
    

    def resultSceneByFileId(file_id):
        src = getFileId(file_id);
        if (src is None): return None;
        dst = result_scenes[src['out_index']]
        return dst;
    
    # link across scenes now:
    for (index,transform) in enumerate( all_transforms ):
        toScene = result_scenes[index];
        for child in transform['m_Children']:
            childResult = resultSceneByFileId(child['fileID'])
            toScene['children'].append(childResult);
        fatherScene = resultSceneByFileId(transform['m_Father']['fileID']);
        if (fatherScene is None):
            root_obj = toScene;

    return root_obj;

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
                #assert(False);
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
    #if (fileExists(jsonPath)): return jsonPath;
    obj = jsonObjFromUnityFile(unityPath);
    obj = sceneThreeFromJsonScene(obj, None);
    writeJsonToFile(obj, jsonPath);
    return jsonPath;

fileToExport = "UnityProject/VRBiofield/Assets/BiofieldCore/ModelPerson/Yogi/Yoga Pose.prefab"
ensureJsonFromUnity(fileToExport);

print("Done.");


