# ExternalSort
Implementation of External Sorting Algorithms
Learn more about K-way Merge

Overview
External sorting is designed for sorting massive files that cannot fit into a single machine’s memory. One common variation is K-way sorting, which consists of two main stages:

Splitting – The input file is divided into smaller chunks that can be sorted using a single machine’s memory and CPU.
Merging – The previously sorted chunks are merged into the final output file, utilizing a single machine’s memory and CPU.
Scaling the Process
To enable distributed processing, I introduced an abstraction layer for file access. This assumes that we can:

Read items sequentially
Write items at the end of a file
Skip a certain number of items
A simple implementation of this abstraction uses the local file system, but in practice, it could be replaced by cloud-based blob storage or other storage services.

Implementation Details
Splitting Stage – Currently, this runs on a single machine. However, it can be parallelized using cloud resources by implementing a distributed Splitter that sorts data across multiple machines.
Merging Stage – This is performed on a single machine, as there is no way to scale it further. The merging process relies on the abstracted file access layer.
This approach allows for flexibility in storage solutions while maintaining efficiency in external sorting.

