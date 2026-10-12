using System.Text.Json;
using System.Text.RegularExpressions;

using System.Collections.Generic;
using System.IO;

namespace LunaLib.Text;

public class TranslationsKeeper // be careful of FileNotFoundExceptation and make sure json file is valid for parsing
{
	public Dictionary<string, string> FallBackTranslations {get; private set;} = [];
	public Dictionary<string, string> Translations {get; private set;} = [];
	
	public string LanguagePath = "";
	public string FallBackLanguagePath = ""; // this should include all keys!
	
	public TranslationsKeeper(string languagePath, string fallBackLanguagePath)
	{
		this.LanguagePath = languagePath;
		this.FallBackLanguagePath = fallBackLanguagePath;

		this.UpdateTranslations();
		this.UpdateFallBackTranslations();
	}

	public void UpdateTranslations()
	{
		using FileStream openStream = File.OpenRead(this.LanguagePath);
		
		this.Translations = JsonSerializer.Deserialize<Dictionary<string, string>>(openStream) ?? throw new JsonException("Translation file cannot contain null.");
	}
	
	public void UpdateFallBackTranslations()
	{
		using FileStream openStream = File.OpenRead(this.FallBackLanguagePath);
		
		this.FallBackTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(openStream) ?? throw new JsonException("Fallback translation file cannot contain null.");
	}
	
	public string GetTranslation(string key, string[]? placeIntoTranslation = null, string replaceTextHolder = "$%s")
	{
		string translation = this.Translations.GetValueOrDefault(key, this.FallBackTranslations.GetValueOrDefault(key, "(KeyNotFoundException: Given key to \"TranslationsKeeper\" was not found in current language or fallback language)"));
		int index = 0;
		
		if (placeIntoTranslation is {Length: > 0})
		{
			translation = Regex.Replace(translation, Regex.Escape(replaceTextHolder), m => 
       		{
          		if (index < placeIntoTranslation.Length)
          		{
              		return placeIntoTranslation[index++];
           		}
           		return m.Value; 
       		});
		}
		
		return translation;
	}
}