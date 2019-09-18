grammar SimplicitC;

program: statement+ EOF;
statement: forloop | whileloop | (variabledeclaration SEMICOLON) | (expression SEMICOLON);
blockstatement: statement | (BRACELEFT statement* BRACERIGHT);
expression: functioncall 
	| rawvalueexpression 
	| addexpression
	| lessthanexpression
	| variable 
	| variableassignment;
rawvalueexpression: INTEGER | STRING;
addexpression: expression PLUS expression;
lessthanexpression: expression LESSTHAN expression;
whileloop: WHILEKEYWORD PARENLEFT expression PARENRIGHT blockstatement;
forloop: FORKEYWORD PARENLEFT expression SEMICOLON expression SEMICOLON expression PARENRIGHT blockstatement;
functiondeclaration: datatype IDENTIFIER PARENLEFT PARENRIGHT blockstatement;
functioncall: IDENTIFIER PARENLEFT (expression (COMMA expression)*)? PARENRIGHT;
variabledeclaration: datatype variable EQUALS expression;
variableassignment: variable EQUALS expression;
variable: IDENTIFIER;
datatype: INTKEYWORD | FLOATKEYWORD | BOOLKEYWORD | STRINGKEYWORD | IDENTIFIER;

PARENLEFT: '(';
PARENRIGHT: ')';
BRACELEFT: '{';
BRACERIGHT: '}';
COMMA: ',';
SEMICOLON: ';';
EQUALS: '=';
PLUS: '+';
MINUS: '-';
LESSTHAN: '<';
GREATERTHAN: '>';
IFKEYWORD: [iI][fF];
FORKEYWORD: [fF][oO][rR];
WHILEKEYWORD: [wW][hH][iI][lL][eE];
INTKEYWORD: [iI][nN][tT];
FLOATKEYWORD: [fF][lL][oO][aA][tT];
BOOLKEYWORD: [bB][oO][oO][lL];
STRINGKEYWORD: [sS][tT][rR][iI][nN][gG];
INTEGER: [0-9]+;
STRING: '"' (~'"' | '\\"')* '"';
IDENTIFIER: [a-zA-Z][a-zA-Z0-9_]*;
WS: [ \r\t\n]+ -> skip;