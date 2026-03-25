; Greek Alphabet
; Each letter is registered as a symbol button on the "Greek" tab.
; Layout: 4 rows, going down then across.


class GreekTabPage extends TabPage
{
  __New()
  {
    super.__New( "Greek" )

    this.RegisterButtons()
    this.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSymbol( 1, 1, 1, "Α", "Alpha",   "Α" )
    super.RegisterSymbol( 2, 1, 1, "Β", "Beta",    "Β" )
    super.RegisterSymbol( 3, 1, 1, "Γ", "Gamma",   "Γ" )
    super.RegisterSymbol( 4, 1, 1, "Δ", "Delta",   "Δ" )

    super.RegisterSymbol( 1, 2, 1, "Ε", "Epsilon",  "Ε" )
    super.RegisterSymbol( 2, 2, 1, "Ζ", "Zeta",     "Ζ" )
    super.RegisterSymbol( 3, 2, 1, "Η", "Eta",      "Η" )
    super.RegisterSymbol( 4, 2, 1, "Θ", "Theta",    "Θ" )

    super.RegisterSymbol( 1, 3, 1, "Ι", "Iota",     "Ι" )
    super.RegisterSymbol( 2, 3, 1, "Κ", "Kappa",    "Κ" )
    super.RegisterSymbol( 3, 3, 1, "Λ", "Lambda",   "Λ" )
    super.RegisterSymbol( 4, 3, 1, "Μ", "Mu",       "Μ" )

    super.RegisterSymbol( 1, 4, 1, "Ν", "Nu",       "Ν" )
    super.RegisterSymbol( 2, 4, 1, "Ξ", "Xi",       "Ξ" )
    super.RegisterSymbol( 3, 4, 1, "Ο", "Omicron",  "Ο" )
    super.RegisterSymbol( 4, 4, 1, "Π", "Pi",       "Π" )

    super.RegisterSymbol( 1, 5, 1, "Ρ", "Rho",      "Ρ" )
    super.RegisterSymbol( 2, 5, 1, "Σ", "Sigma",    "Σ" )
    super.RegisterSymbol( 3, 5, 1, "Τ", "Tau",      "Τ" )
    super.RegisterSymbol( 4, 5, 1, "Υ", "Upsilon",  "Υ" )

    super.RegisterSymbol( 1, 6, 1, "Φ", "Phi",      "Φ" )
    super.RegisterSymbol( 2, 6, 1, "Χ", "Chi",      "Χ" )
    super.RegisterSymbol( 3, 6, 1, "Ψ", "Psi",      "Ψ" )
    super.RegisterSymbol( 4, 6, 1, "Ω", "Omega",    "Ω" )
  }
}

