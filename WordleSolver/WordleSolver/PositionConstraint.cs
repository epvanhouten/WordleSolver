namespace WordleSolver;

public class PositionConstraint
{
    private readonly char[] _allowedCharacters;

    public PositionConstraint()
        : this(GameConstants.AllCharacters)
    {

    }

    private PositionConstraint(char[] allowedCharacters)
    {
        _allowedCharacters = allowedCharacters;
    }

    public PositionConstraint ForbidCharacter(char forbiddenCharacter)
    {
        if (_allowedCharacters.Length == 1)
        {
            return this;
        }

        var newSet = _allowedCharacters.Where(c => c != forbiddenCharacter).ToArray();
        return new PositionConstraint(newSet);
    }

    public PositionConstraint SetExact(char exactCharacter)
    {
        return new PositionConstraint(new[] { exactCharacter });
    }

    public bool IsCharacterAllowed(char c)
    {
        if (_allowedCharacters == GameConstants.AllCharacters)
        {
            return true;
        }

        var originalIndex = c - 'a';
        var worstCaseIndex = originalIndex - (GameConstants.AllCharacters.Length - _allowedCharacters.Length);
        worstCaseIndex = worstCaseIndex < 0 ? 0 : worstCaseIndex;
        for (var searchIndex = worstCaseIndex; searchIndex < _allowedCharacters.Length && searchIndex <= originalIndex; searchIndex++)
        {
            if (_allowedCharacters[searchIndex] == c)
            {
                return true;
            }
        }

        return false;
    }
}