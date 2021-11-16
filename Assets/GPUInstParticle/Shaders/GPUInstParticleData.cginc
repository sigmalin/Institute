#ifndef _GPU_INSTANCE_PARTICLE_DATA_H
#define _GPU_INSTANCE_PARTICLE_DATA_H

struct particle
{
	float3 position;
	float3 color;
	float3 velocity;
	float scale;
	float lifeTime;
	float delayTime;
};

#endif
