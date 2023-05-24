namespace Contracts.v1.DataPoint;

public record FormulaDto
{
    public string Operation { get; set; }
    public double Factor { get; set; }
    
}