#!/usr/bin/python
# -*- coding: utf-8 -*-
import math
import sys
import random
import subprocess

def solveIt(inputData):
    # Modify this code to run your optimization algorithm
    
    fileName = [r'C:\Users\mavm01\Dropbox\Discrete Optimization\VRP2\VRPConsole\bin\Release\VRPConsole.exe', 'python', '100000']
    p = subprocess.Popen(fileName, stdin=subprocess.PIPE, stdout=subprocess.PIPE)
    (out, err) = p.communicate(inputData)
    p.wait()
    return out



import sys

if __name__ == '__main__':
    if len(sys.argv) > 1:
        fileLocation = sys.argv[1].strip()
        inputDataFile = open(fileLocation, 'r')
        inputData = ''.join(inputDataFile.readlines())
        inputDataFile.close()
        print 'Solving:', fileLocation
        print solveIt(inputData)
    else:
        print 'This test requires an input file.  Please select one from the data directory. (i.e. python solver.py ./data/wl_16_1)'

