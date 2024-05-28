
print("Unity to JSON exporter...");

GlobalJsonOutputPath = "docs/biofield_json/export/";
GlobalSceneOutputPath = "docs/biofield_json/scene/";
finalAllMetaFile = "tools/all_meta.json";
All_By_Guids = {};

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
            prefabPostfix = " stripped";
            isPrefab = False;
            if (prefabPostfix in id):
                id = id.replace(prefabPostfix,"");
                isPrefab = True;
            activePart = [];
            item = { 'fileID':id, 'yaml':activePart }
            if (isPrefab):
                item['is_prefab'] = True;
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
        #print("ResultText=", resultText);
        print("Parsing...", partIndex, " of ", partCount);
        partIndex = partIndex + 1;
        data = yaml.load(resultText, Loader=yaml.SafeLoader)
        #print("Resolved.");
        fileID = part['fileID'];
        if ('is_prefab' in part):
            first_key = list(data.keys())[0];
            data[first_key]['is_prefab'] = True;

        assert(not(fileID in byFileId));
        byFileId[fileID] = data;
    return byFileId;

All_Json_By_Path = {};

def jsonPathFromUnityPath(unityPath):
    cleanPath = unityPath.replace("/","_").replace(" ","_");
    cleanPath = cleanPath.replace("UnityProject/VRBiofield/Assets/","");
    jsonPath = GlobalJsonOutputPath + cleanPath + ".json";
    return jsonPath;

def scenePathFromUnityPath(unityPath):
    jsonPath = jsonPathFromUnityPath(unityPath);
    scenePath = jsonPath.replace(GlobalJsonOutputPath,GlobalSceneOutputPath);
    return scenePath;

def ensureJsonFromUnityPath(unityPath):
    if (unityPath in All_Json_By_Path):
        return All_Json_By_Path[unityPath];
    jsonPath = jsonPathFromUnityPath(unityPath);
    if (fileExists(jsonPath)):
        print("Using cached json for ", jsonPath);
        All_Json_By_Path[unityPath] = readAllJson(jsonPath);
        return All_Json_By_Path[unityPath];
    print("Caching ", unityPath, " to ", jsonPath);
    jsonObj = jsonObjFromUnityFile(unityPath);
    writeJsonToFile(jsonObj, jsonPath);
    All_Json_By_Path[unityPath] = jsonObj;
    return jsonObj;

def isUnityPathParseable(path):
    if (path.endswith(".unity") or path.endswith(".prefab")):
        return True;
    print("Can't parse ", path);
    return False;

def valOfFirstKey(dct):
    first_key = list(dct.keys())[0];
    first_val = dct[first_key];
    return first_val;

def setPropertyByPath(obj, path, val):
    while ('.' in path):
        dotIndex = path.find(".");
        left = path[:dotIndex];
        if (left == "Array"):
            print("Ignoring Array in prefab for now...");
            return; # ignore for now

        if (not(left in obj)):
            obj[left] = {};
        obj = obj[left];
        right = path[dotIndex+1:];
        path = right;
    obj[path] = val;

def applyPrefab(into, prefab, mods):

    prefab = valOfFirstKey(prefab);
    for k in prefab.keys():
        into[k] = prefab[k];
    
    modTop = valOfFirstKey(mods)['m_Modification'];
    modTransformParent = modTop['m_TransformParent'];
    into['m_Father'] = modTransformParent;
    modList = modTop['m_Modifications'];
    for mod in modList:
        modPath = mod['propertyPath'];
        setPropertyByPath(into, modPath, mod['value']);
    pass;

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
        first_val["type"] = first_key;
        first_val["fileID"] = file_id;
        if (not(first_type in by_types)):
            by_types[first_type] = [];
        by_types[first_type].append(first_val);
    
    for (index,file_id) in enumerate(component_by_file_id):
        with_type = component_by_file_id[file_id];
        (typeName,obj) = typeNameAndObject(with_type);
        if ('is_prefab' in obj):
            internalId = str(obj['m_PrefabInternal']['fileID']);
            internalObj = component_by_file_id[internalId];
            externalGuid = obj['m_PrefabParentObject']['guid'];
            externalFileId = obj['m_PrefabParentObject']['fileID'];
            externalObj = object_by_guid[str(externalGuid)];
            externalPath = externalObj['path'].replace(".meta","");
            if (isUnityPathParseable(externalPath)):
                externalScene = ensureJsonFromUnityPath(externalPath);
                externalComp = externalScene[str(externalFileId)];
                applyPrefab(obj, externalComp, internalObj);
                #obj['prefab_base'] = externalComp;
                #obj['prefab_inst'] = internalObj;
            else:
                print("Unsupported prefab type.");
    
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
        components = {"GameObject":gameObj};
        scene = { 
            'name':gameObj['m_Name'],
            'children':[],
            "position":listFromDictVector(transform['m_LocalPosition']),
            "quaternion":listFromDictVector(transform['m_LocalRotation']),
            "scale":listFromDictVector(transform['m_LocalScale']),
            "userData":{"components":components},
        };
        result_scenes.append(scene);

        # todo: check prefab if gameObj has [GameObject][m_PrefabParentObject][guid]
        # todo: check [MeshFilter][m_Mesh][guid], maybe [MeshRenderer]
        # todo: custom types: FlowVertexNode, 

        # get components for userData:
        for comp in gameObj['m_Component']:
            (typeIndex,ptr) = typeNameAndObject(comp);
            obj = getFileId(ptr);
            objType = obj["type"];
            if (objType == "MonoBehaviour"):
                scriptGuid = obj['m_Script']['guid'];
                if (scriptGuid in All_By_Guids):
                    scriptInfo = All_By_Guids[scriptGuid];
                    scriptName = str(scriptInfo['path']);
                    scriptName = scriptName.split('/')[-1];
                    scriptName = scriptName.split(".")[0];
                    objType = scriptName;
                pass; # todo: find the real script name: .m_Script.guid
            while (objType in components):
                objType = objType + "_";
            components[objType] = obj;
        
    

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

def readAllJson(path):
    with open(path) as f:
        data = json.load(f);
    return data;

All_By_Guids = readAllJson(finalAllMetaFile)['by_guid'];

def ensureSceneFromUnity(unityPath):
    scenePath = scenePathFromUnityPath(unityPath);
    if (fileExists(scenePath)): return scenePath;

    jsonObj = ensureJsonFromUnityPath(unityPath);
    obj = sceneThreeFromJsonScene(jsonObj, All_By_Guids);
    writeJsonToFile(obj, scenePath);
    return scenePath;

def ensureFilesExporter():
    filesToProcess = [
        "UnityProject/VRBiofield/Assets/BiofieldCore/ModelPerson/Yogi/Yoga Pose.prefab",
        "UnityProject/VRBiofield/Assets/BiofieldCore/ModelPerson/Hands/Hand System.prefab",
        #"UnityProject/VRBiofield/Assets/ChiVR_MainApp_Chakras.unity"
    ];
    for fileToExport in filesToProcess:
        ensureSceneFromUnity(fileToExport);
    print("Files updated.");

ensureFilesExporter();

print("Done.");


