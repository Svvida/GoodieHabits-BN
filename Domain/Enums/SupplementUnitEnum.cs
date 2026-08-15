namespace Domain.Enums
{
    /// <summary>
    /// Unit a supplement is dosed in. Lives on the <c>Supplement</c>, never on a schedule slot — a supplement
    /// whose slots disagreed about the unit would make every aggregate over its intakes meaningless. Same
    /// inheritance call as a finance sub-category inheriting its parent's type.
    /// </summary>
    public enum SupplementUnitEnum
    {
        Piece,
        Capsule,
        Tablet,
        Gram,
        Milligram,
        Milliliter,
        Scoop,
        Drop,
        InternationalUnit,
    }
}
