using System;
using Avalonia.Input;

namespace EggMdEditor.App;

/// <summary>
/// Handler de atajos de teclado de SC-001 (ENT-007, OP-005, COM-001):
/// traduce la pulsación a la combinación registrada y la despacha por el
/// dispatcher compartido. Las combinaciones sin binding, con el atajo
/// inactivo o con una acción que no existe en F-001 no producen efecto
/// observable (BR-F001-003, ADR-001).
/// </summary>
public sealed class ShortcutHandler
{
    private readonly CommandDispatcher _dispatcher;

    public ShortcutHandler(CommandDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// OP-005: resuelve y ejecuta el atajo de la pulsación. Devuelve
    /// <c>true</c> solo cuando se ejecutó una acción disponible en F-001.
    /// </summary>
    public bool TryHandle(KeyEventArgs e) => TryHandle(e, out _);

    public bool TryHandle(KeyEventArgs e, out Shortcut? shortcut)
    {
        ArgumentNullException.ThrowIfNull(e);

        shortcut = _dispatcher.ResolveShortcut(GetCombination(e.Key, e.KeyModifiers, e.PhysicalKey));
        if (shortcut is null)
        {
            return false;
        }

        if (!_dispatcher.DispatchShortcut(shortcut))
        {
            shortcut = null;
            return false;
        }

        return true;
    }

    /// <summary>OP-005 por combinación textual (p. ej. <c>Alt+A</c>).</summary>
    public bool TryHandle(string? combination) => _dispatcher.DispatchShortcut(combination);

    /// <summary>
    /// Traduce una pulsación a la combinación canónica de ENT-007. Solo las
    /// combinaciones con modificador y Esc son atajos: una tecla suelta no se
    /// consume para no interferir con la escritura futura del documento.
    /// </summary>
    public static string? GetCombination(Key key, KeyModifiers modifiers, PhysicalKey physicalKey = PhysicalKey.None)
    {
        var name = ResolveKeyName(key, physicalKey);
        if (name is null)
        {
            return null;
        }

        var prefix = BuildModifierPrefix(modifiers);
        if (prefix.Length == 0 && !string.Equals(name, "Esc", StringComparison.Ordinal))
        {
            return null;
        }

        return prefix + name;
    }

    private static string BuildModifierPrefix(KeyModifiers modifiers)
    {
        var prefix = string.Empty;
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            prefix += "Ctrl+";
        }

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            prefix += "Alt+";
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            prefix += "Shift+";
        }

        if (modifiers.HasFlag(KeyModifiers.Meta))
        {
            prefix += "Meta+";
        }

        return prefix;
    }

    private static string? ResolveKeyName(Key key, PhysicalKey physicalKey)
    {
        if (key == Key.Escape)
        {
            return "Esc";
        }

        if (key is Key.None or Key.System)
        {
            // Algunas plataformas reportan Key.System al pulsar Alt+letra:
            // la posición física sigue identificando la letra.
            return IsLetterOrDigitName(physicalKey.ToString());
        }

        var name = key.ToString();
        if (name.Length == 1 && char.IsAsciiLetter(name[0]))
        {
            return name.ToUpperInvariant();
        }

        if (name.Length == 2 && name[0] == 'D' && char.IsAsciiDigit(name[1]))
        {
            return name[1..];
        }

        return IsLetterOrDigitName(name) ?? name;
    }

    private static string? IsLetterOrDigitName(string? name)
    {
        if (string.IsNullOrEmpty(name) || name.Length != 1)
        {
            return null;
        }

        var character = name[0];
        if (char.IsAsciiLetter(character))
        {
            return char.ToUpperInvariant(character).ToString();
        }

        return char.IsAsciiDigit(character) ? character.ToString() : null;
    }
}
