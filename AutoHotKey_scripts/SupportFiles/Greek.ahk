; Greek Alphabet
; Each letter is registered as a symbol button on the "Greek" tab.
; Layout: 4 rows, going down then across.


class GreekTabPage extends TabPage
{
  __New()
  {
    super.__New( "Greek" )
    this .m_leftGap := 2
    super.SetByCol( 12 + this.m_leftGap )
    this .RegisterButtons()
    super.RecalcSizes()
  }

  RegisterButtons()
  {
    super.RegisterSpace( this.m_maxRowsOrCols )

    super.RegisterSpace( this.m_leftGap )
    super.RegisterSymbolX( 1, "Α", "Uppercase Alpha"   )
    super.RegisterSymbolX( 1, "Β", "Uppercase Beta"    )
    super.RegisterSymbolX( 1, "Γ", "Uppercase Gamma"   )
    super.RegisterSymbolX( 1, "Δ", "Uppercase Delta"   )
    super.RegisterSymbolX( 1, "Ε", "Uppercase Epsilon" )
    super.RegisterSymbolX( 1, "Ζ", "Uppercase Zeta"    )
    super.RegisterSymbolX( 1, "Η", "Uppercase Eta"     )
    super.RegisterSymbolX( 1, "Θ", "Uppercase Theta"   )
    super.RegisterSymbolX( 1, "Ι", "Uppercase Iota"    )
    super.RegisterSymbolX( 1, "Κ", "Uppercase Kappa"   )
    super.RegisterSymbolX( 1, "Λ", "Uppercase Lambda"  )
    super.RegisterSymbolX( 1, "Μ", "Uppercase Mu"      )

    super.RegisterSpace( this.m_leftGap )
    super.RegisterSymbolX( 1, "Ν", "Uppercase Nu"      )
    super.RegisterSymbolX( 1, "Ξ", "Uppercase Xi"      )
    super.RegisterSymbolX( 1, "Ο", "Uppercase Omicron" )
    super.RegisterSymbolX( 1, "Π", "Uppercase Pi"      )
    super.RegisterSymbolX( 1, "Ρ", "Uppercase Rho"     )
    super.RegisterSymbolX( 1, "Σ", "Uppercase Sigma"   )
    super.RegisterSymbolX( 1, "Τ", "Uppercase Tau"     )
    super.RegisterSymbolX( 1, "Υ", "Uppercase Upsilon" )
    super.RegisterSymbolX( 1, "Φ", "Uppercase Phi"     )
    super.RegisterSymbolX( 1, "Χ", "Uppercase Chi"     )
    super.RegisterSymbolX( 1, "Ψ", "Uppercase Psi"     )
    super.RegisterSymbolX( 1, "Ω", "Uppercase Omega"   )

    super.RegisterSpace( this.m_maxRowsOrCols )

    super.RegisterSpace( this.m_leftGap )
    super.RegisterSymbolX( 1, "α", "Lowercase Alpha"   )
    super.RegisterSymbolX( 1, "β", "Lowercase Beta"    )
    super.RegisterSymbolX( 1, "γ", "Lowercase Gamma"   )
    super.RegisterSymbolX( 1, "δ", "Lowercase Delta"   )
    super.RegisterSymbolX( 1, "ε", "Lowercase Epsilon" )
    super.RegisterSymbolX( 1, "ζ", "Lowercase Zeta"    )
    super.RegisterSymbolX( 1, "η", "Lowercase Eta"     )
    super.RegisterSymbolX( 1, "θ", "Lowercase Theta"   )
    super.RegisterSymbolX( 1, "ι", "Lowercase Iota"    )
    super.RegisterSymbolX( 1, "κ", "Lowercase Kappa"   )
    super.RegisterSymbolX( 1, "λ", "Lowercase Lambda"  )
    super.RegisterSymbolX( 1, "μ", "Lowercase Mu"      )

    super.RegisterSpace( this.m_leftGap )
    super.RegisterSymbolX( 1, "ν", "Lowercase Nu"      )
    super.RegisterSymbolX( 1, "ξ", "Lowercase Xi"      )
    super.RegisterSymbolX( 1, "ο", "Lowercase Omicron" )
    super.RegisterSymbolX( 1, "π", "Lowercase Pi"      )
    super.RegisterSymbolX( 1, "ρ", "Lowercase Rho"     )
    super.RegisterSymbolX( 1, "σ", "Lowercase Sigma"   )
    super.RegisterSymbolX( 1, "τ", "Lowercase Tau"     )
    super.RegisterSymbolX( 1, "υ", "Lowercase Upsilon" )
    super.RegisterSymbolX( 1, "φ", "Lowercase Phi"     )
    super.RegisterSymbolX( 1, "χ", "Lowercase Chi"     )
    super.RegisterSymbolX( 1, "ψ", "Lowercase Psi"     )
    super.RegisterSymbolX( 1, "ω", "Lowercase Omega"   )

  }
}

