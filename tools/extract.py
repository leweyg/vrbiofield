
print("Unity to JSON exporter...");

finalResultPath = "tools/all_meta.json";

srcYamlPath = "UnityProject/VRBiofield/Assets/Scenes.meta";
import yaml
def jsonObjFromYamlPath(yamlPath):
    data = None;
    with open(yamlPath, 'r') as f:
        data = yaml.load(f, Loader=yaml.SafeLoader)
        #print("data=", data);
    return data;
        
#print(jsonObjFromYamlPath(srcYamlPath))

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
    return allYamlsByGuid;

buildAllJson();

import json
def writeJsonToFile(obj, path):
    with open(path, 'w') as f:
        json.dump(obj, f)

writeJsonToFile(finalResult, finalResultPath);

print("Done.");


