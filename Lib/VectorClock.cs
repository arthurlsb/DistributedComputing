namespace Lib;

// https://decomposition.al/blog/2022/08/11/an-example-run-of-a-matrix-based-causal-unicast-protocol/

public class VectorClock
{
    public int[,] Senti = new int[5, 5];

    public int[] Delivi = new int[5];

    public VectorClock()
    {
        for (int i = 0; i < Delivi.Length; i++)
        {
            Delivi[i] = 0;
            for (int j = 0; j < Delivi.Length; j++)
            {
                Senti[i, j] = 0;
            }
        }
    }

    public void IncrementSenti(int idSender, int idReceiver)
    {
        Senti[idSender, idReceiver]++;
    }

    public void Update(int[,] sentiReceived, int idReceiver, int idSender)
    {
        Delivi[idSender]++;
        Senti[idSender, idReceiver]++;

        for (int i = 0; i < Delivi.Length; i++)
        {
            for (int j = 0; j < Delivi.Length; j++)
            {
                Senti[i, j] = Math.Max(Senti[i, j], sentiReceived[i, j]);
            }
        }
    }

    public bool IsCausal(int[,] sentiReceived, int idReceiver)
    {
        int verifier = 0;

        for (int i = 0; i < Delivi.Length; i++)
        {
          
            for (int j = 0; j < Delivi.Length; j++)
            {
                if (j == idReceiver)
                {
                    if(Delivi[i] >= sentiReceived[i, j])
                    {
                        verifier++;
                    }
                }
            }  
        }

        if (verifier == Delivi.Length)
        {
            return true;
        }
        
        return false;
    }
}
