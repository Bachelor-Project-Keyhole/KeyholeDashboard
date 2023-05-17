using Domain.Exceptions;

namespace Domain.Datapoint;

public class Formula
{
    public MathOperation Operation { get; set; }
    public double Factor { get; set; }

    public Formula(MathOperation operation = MathOperation.None, double factor = 0)
    {
        if (operation == MathOperation.Divide && factor == 0)
            throw new CannotDivideWithZeroException();
        Operation = operation;
        Factor = factor;
    }
}