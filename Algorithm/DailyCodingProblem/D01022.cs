namespace DailyCodingProblem;

/// <summary>
/// Given an array of integers where every integer occurs three times except for one integer,
/// which only occurs once, find and return the non-duplicated integer.
/// For example, given [6, 1, 3, 3, 3, 6, 6], return 1. Given [13, 19, 13, 13], return 19.
/// Do this in O(N) time and O(1) space.
/// </summary>
public class D01022
{
    public static void Exection()
    {   
        /// ab表示三进制，因此只有00，01，10，ra/rb表示%3的结果。
        /// a	b	c	rA	rB
        /// 0	0	0	0	0
        /// 0	1	0	0	1
        /// 1	0	0	1	0

        /// 0	0	1	0	1
        /// 0   1	1	1	0
        /// 1	0	1	0	0

        /// 真值表求逻辑代数的求解方法
        /// rA = (a&~b&~c) | (~a&b&c)
        /// rB = (~a&b&~c) | (~a&~b&c)
        var nums = new int[] {6,6,6,3,3,3,4}; 
        var a = 0;
        var b = 0;
        var rA = 0;
        var rB = 0;
        foreach (var c in nums)
        {
            rA = (a&~b&~c) | (~a&b&c);
            rB = (~a&b&~c) | (~a&~b&c);
            a = rA;
            b = rB;
        }
        Console.WriteLine(rB);
    }
}