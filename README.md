# ExternalSort
Implementation of External sorting algorithms
https://en.wikipedia.org/wiki/K-way_merge_algorithm

External sorting designed to sort huge files which cannot fit memory of single machine.
Here one of variations of external sorting Kway sorting 
Consists 2 stages 
	1 Split input file into smaller files which can be sorted on single machine memory and CPU
	2 Merge previously sorted files into output file using single machine memory and CPU
In order to scale processes using distributed storage and CPU resources I added abstract layer for
accessing files. With assumption that we can sequentially read items and write them to the end, also we can skip
some number of items.

There is a simple implementation of this abstraction using local machine file system.
In practice it could be cloud blob storage service or other storage service.

Split stage implementation is simple using single machine, but it could be run in parallel using cloud resources.
For parallel need to reimplement Splitter which run distributed sorting on several machines. 

Merge implementation is on single machine. Unfortunately there is no way to scale it. It use abstract way to read files.

