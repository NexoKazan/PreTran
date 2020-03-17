#region Listener
		private int _tmpDepth;
        private int _depth;
        private bool _isMainQ = false;
        private bool _isFirst = true;
        private int _isOtherListener = 1;

        public List<BaseRule> Rules = new List<BaseRule>();

        public override void VisitTerminal(ITerminalNode node)
        {
            if (_isOtherListener==1)
            {
                TerminalRule terminal = new TerminalRule(node.SourceInterval, node.GetText(), node.Parent);
                Rules.Add(terminal);
            }
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if (context.ChildCount > 1)
            {
                if(_isOtherListener == 1)
                    Rules.Add(new BaseRule(context.SourceInterval, context, context.GetText()));
            }
        }
#endregion

ParseTreeWalker walker = new ParseTreeWalker();
walker.Walk(_listener, context);
Rules = _listener.Rules;
foreach (var rule in Rules)
{
    rule.Text += " ";
}


if (_isOtherListener == 1 && Rules.Count > 0 && _isFirst)
{
    Rules.Remove(Rules[Rules.Count - 1]);
    _isFirst = false;
}


if (_isOtherListener == 1)
{
if (context.ChildCount > 1)
{
    Rules.Remove(Rules[Rules.Count - 1]);
}

ComparisonOperator comparisonOperator =
    new ComparisonOperator(context.SourceInterval, context, context.GetText());

Rules.Add(comparisonOperator);

}
_isOtherListener++;