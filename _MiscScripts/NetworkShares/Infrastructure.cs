using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkShares;

/// <summary>A minimal ICommand. CanExecute re-queries via CommandManager.</summary>
internal sealed class RelayCommand : ICommand
{
  private readonly Action       m_execute;
  private readonly Func<bool>?  m_canExecute;

  public RelayCommand( Action execute, Func<bool>? canExecute = null )
  {
    m_execute    = execute;
    m_canExecute = canExecute;
  }

  public bool CanExecute( object? parameter ) => m_canExecute?.Invoke() ?? true;
  public void Execute( object? parameter )    => m_execute();

  public event EventHandler? CanExecuteChanged
  {
    add    => CommandManager.RequerySuggested += value;
    remove => CommandManager.RequerySuggested -= value;
  }
}

/// <summary>
/// Attached properties to two-way bind a <see cref="PasswordBox"/> to a string on a
/// view model (PasswordBox.Password isn't a DependencyProperty for security reasons).
/// Usage: &lt;PasswordBox local:PasswordHelper.Attach="True"
///                       local:PasswordHelper.BoundPassword="{Binding Password, Mode=TwoWay}"/&gt;
/// </summary>
internal static class PasswordHelper
{
  private static readonly DependencyProperty UpdatingProperty =
    DependencyProperty.RegisterAttached( "Updating", typeof( bool ), typeof( PasswordHelper ), new PropertyMetadata( false ) );

  public static readonly DependencyProperty BoundPasswordProperty =
    DependencyProperty.RegisterAttached( "BoundPassword", typeof( string ), typeof( PasswordHelper ),
      new FrameworkPropertyMetadata( string.Empty, OnBoundPasswordChanged ) );

  public static readonly DependencyProperty AttachProperty =
    DependencyProperty.RegisterAttached( "Attach", typeof( bool ), typeof( PasswordHelper ),
      new PropertyMetadata( false, OnAttachChanged ) );

  public static string GetBoundPassword( DependencyObject d ) => (string)d.GetValue( BoundPasswordProperty );
  public static void   SetBoundPassword( DependencyObject d, string v ) => d.SetValue( BoundPasswordProperty, v );
  public static bool   GetAttach( DependencyObject d ) => (bool)d.GetValue( AttachProperty );
  public static void   SetAttach( DependencyObject d, bool v ) => d.SetValue( AttachProperty, v );

  private static void OnAttachChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
  {
    if( d is not PasswordBox box )
    {
      return;
    }
    if( (bool)e.OldValue ) box.PasswordChanged -= OnPasswordChanged;
    if( (bool)e.NewValue ) box.PasswordChanged += OnPasswordChanged;
  }

  private static void OnBoundPasswordChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
  {
    if( d is PasswordBox box && !(bool)box.GetValue( UpdatingProperty ) && box.Password != (string)e.NewValue )
    {
      box.Password = (string)e.NewValue;
    }
  }

  private static void OnPasswordChanged( object sender, RoutedEventArgs e )
  {
    var box = (PasswordBox)sender;
    box.SetValue( UpdatingProperty, true );
    SetBoundPassword( box, box.Password );
    box.SetValue( UpdatingProperty, false );
  }
}
