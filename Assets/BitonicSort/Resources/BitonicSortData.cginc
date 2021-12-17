#ifndef _BITONIC_SORT_DATA_H
#define _BITONIC_SORT_DATA_H

struct BitonicSortData
{
	float key;
	uint index;
};

#define data_t BitonicSortData
#define COMPARISON(a,b) ( a.key > b.key )

#endif
