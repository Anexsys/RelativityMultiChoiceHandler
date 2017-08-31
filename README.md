# RelativityMultiChoiceHandler

## Built by Anexsys

### Usage
Use the MultiChoiceHelpers class to generate a MultiChoiceFieldValueList object from a list of strings. If the choices do not exist in the workspace in Relativity, they will be created.

```C#
string FIELD_NAME = "Field Name";

List<string> fieldChoices = new List<string>()
{
    "A",
    "B",
    "C",
    "D"
};

MultiChoiceFieldValueList values = MultiChoiceHelpers.CreateAndGetMultiChoices(dbc, proxy, FIELD_NAME, fieldChoices);
```

![screenshot](https://github.com/Anexsys/RelativityMultiChoiceHandler/blob/master/resources/multichoices1.png)


Nested choices can also be created:
```C#
List<string> fieldChoices = new List<string>()
{
    "A/Aa/Aardvark",
    "A/Ap/Apple",
    "A/An/Anexsys",
    "B/Ba/Bat"
}
```

![screenshot](https://github.com/Anexsys/RelativityMultiChoiceHandler/blob/master/resources/multichoices2.png)
