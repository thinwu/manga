import os
import sys
import shutil
import re 
walk_dir = r"D:\capture\thinwugithub\manga\aaa"
output_dir = r"D:\capture\thinwugithub\manga\aaa.output"
#
# used to put all the files in to one folder with a prefix
# as the prestep of compressing comic images
#
print (walk_dir)

if not os.path.exists(output_dir):
    os.makedirs(output_dir)


for root, subdirs, files in os.walk(walk_dir):
    intNum = 1
    for file in files:
        strNum=str(intNum)
        print (root)
        strNum = str(float(strNum)/1000).replace(".","")
        if len(strNum)==3:
            strNum = strNum+"0"
        elif len(strNum)==2:
            strNum = strNum+"00"
        elif len(strNum)==1:
            strNum = strNum+"000"
        list_file_path = os.path.join(root,file)
        fileSurfix = file.split(".")[-1]
        print (list_file_path)
        try:
            parentFolder = list_file_path.split("\\")[-2]
            newFileName = parentFolder+"."+strNum + "." + fileSurfix
            newFilePath = os.path.join(output_dir,newFileName)
            print(newFilePath)
            if not os.path.isfile(newFilePath):
                shutil.copyfile(list_file_path, newFilePath)
        except:
            print ("Except: " + list_file_path)
        intNum = intNum + 1