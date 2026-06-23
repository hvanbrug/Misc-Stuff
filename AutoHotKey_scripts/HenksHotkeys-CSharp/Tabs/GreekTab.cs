using HenksHotkeys.UI;

namespace HenksHotkeys.Tabs;

/// <summary>Greek alphabet tab (Greek.ahk). Two-slot left gap, 12 letters per block.</summary>
internal sealed class GreekTab : TabModel
{
  private const int LeftGap = 2;

  public GreekTab() : base( "Greek" )
  {
    SetRowsOf( 12 + LeftGap );
    RegisterButtons();
    RecalcSizes();
  }

  private void RegisterButtons()
  {
    NextLine();

    RegisterSpace( LeftGap );
    RegisterSymbolX( 1, "Α", "Uppercase Alpha" );
    RegisterSymbolX( 1, "Β", "Uppercase Beta" );
    RegisterSymbolX( 1, "Γ", "Uppercase Gamma" );
    RegisterSymbolX( 1, "Δ", "Uppercase Delta" );
    RegisterSymbolX( 1, "Ε", "Uppercase Epsilon" );
    RegisterSymbolX( 1, "Ζ", "Uppercase Zeta" );
    RegisterSymbolX( 1, "Η", "Uppercase Eta" );
    RegisterSymbolX( 1, "Θ", "Uppercase Theta" );
    RegisterSymbolX( 1, "Ι", "Uppercase Iota" );
    RegisterSymbolX( 1, "Κ", "Uppercase Kappa" );
    RegisterSymbolX( 1, "Λ", "Uppercase Lambda" );
    RegisterSymbolX( 1, "Μ", "Uppercase Mu" );

    RegisterSpace( LeftGap );
    RegisterSymbolX( 1, "Ν", "Uppercase Nu" );
    RegisterSymbolX( 1, "Ξ", "Uppercase Xi" );
    RegisterSymbolX( 1, "Ο", "Uppercase Omicron" );
    RegisterSymbolX( 1, "Π", "Uppercase Pi" );
    RegisterSymbolX( 1, "Ρ", "Uppercase Rho" );
    RegisterSymbolX( 1, "Σ", "Uppercase Sigma" );
    RegisterSymbolX( 1, "Τ", "Uppercase Tau" );
    RegisterSymbolX( 1, "Υ", "Uppercase Upsilon" );
    RegisterSymbolX( 1, "Φ", "Uppercase Phi" );
    RegisterSymbolX( 1, "Χ", "Uppercase Chi" );
    RegisterSymbolX( 1, "Ψ", "Uppercase Psi" );
    RegisterSymbolX( 1, "Ω", "Uppercase Omega" );

    ShiftLineByThird();

    RegisterSpace( LeftGap );
    RegisterSymbolX( 1, "α", "Lowercase Alpha" );
    RegisterSymbolX( 1, "β", "Lowercase Beta" );
    RegisterSymbolX( 1, "γ", "Lowercase Gamma" );
    RegisterSymbolX( 1, "δ", "Lowercase Delta" );
    RegisterSymbolX( 1, "ε", "Lowercase Epsilon" );
    RegisterSymbolX( 1, "ζ", "Lowercase Zeta" );
    RegisterSymbolX( 1, "η", "Lowercase Eta" );
    RegisterSymbolX( 1, "θ", "Lowercase Theta" );
    RegisterSymbolX( 1, "ι", "Lowercase Iota" );
    RegisterSymbolX( 1, "κ", "Lowercase Kappa" );
    RegisterSymbolX( 1, "λ", "Lowercase Lambda" );
    RegisterSymbolX( 1, "μ", "Lowercase Mu" );

    RegisterSpace( LeftGap );
    RegisterSymbolX( 1, "ν", "Lowercase Nu" );
    RegisterSymbolX( 1, "ξ", "Lowercase Xi" );
    RegisterSymbolX( 1, "ο", "Lowercase Omicron" );
    RegisterSymbolX( 1, "π", "Lowercase Pi" );
    RegisterSymbolX( 1, "ρ", "Lowercase Rho" );
    RegisterSymbolX( 1, "σ", "Lowercase Sigma" );
    RegisterSymbolX( 1, "τ", "Lowercase Tau" );
    RegisterSymbolX( 1, "υ", "Lowercase Upsilon" );
    RegisterSymbolX( 1, "φ", "Lowercase Phi" );
    RegisterSymbolX( 1, "χ", "Lowercase Chi" );
    RegisterSymbolX( 1, "ψ", "Lowercase Psi" );
    RegisterSymbolX( 1, "ω", "Lowercase Omega" );
  }
}
