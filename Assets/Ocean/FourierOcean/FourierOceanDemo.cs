using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourierOceanDemo : FourierOcean_Projection//FourierOcean_Matrices
{
    public enum FourierOceanType
    {
        FastFourier,
        DiscreteFourier,
    }

    public FourierOceanType Type;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Initialize();
    }

    void Initialize()
    {
        switch(Type)
        {
            case FourierOceanType.FastFourier:
                InitFourierOceanCore(new FastFourierOceanCore());
                break;
            case FourierOceanType.DiscreteFourier:
                InitFourierOceanCore(new DiscreteFourierOceanCore());
                break;
        }
    }
}
