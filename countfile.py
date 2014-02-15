import os

def countFile(file):
	numlines = 0
	f = open(file)
	for line in f:
		numlines += 1
	f.close()
	return numlines

val = 0
fct = 0
for path, subdirs, files in os.walk(os.getcwd()):
	for filename in files:
		f = os.path.join(path, filename)
		n, ext = os.path.splitext(f)
		if ext != '.cs':
			continue
		val += countFile(f)
		fct += 1

print("Found " + str(fct) + " files with " + str(val) + " lines total.")

input("enter");