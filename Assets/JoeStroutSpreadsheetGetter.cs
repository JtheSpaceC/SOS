/* This class provides access to Google Sheets data.  It assumes that
    a Google Script, deployed as a web app, looks something like the
    code at the bottom of this file.
    */

using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Experimental.Networking;
using System.Collections;
using System.Collections.Generic;

public static class JoeStroutSpreadsheetGetter {
	/*
	// Here's the "exec" URL for actually running the web service...
	public static string serviceURL = "https://script.google.com/macros/s/blahblahblahYourURLHere/exec";
	// And the source code is at:
	// https://script.google.com/a/macros/somedomain/d/blahblahblahYourURLHere/edit

	public static UnityWebRequest BuildRequestForSheet(string sheetName) {

		Dictionary<string, string> form = new Dictionary<string, string>();
		form["ssid"] = "yourSSIDhere";
		form["pass"] = "yourPassword";
		form["sheet"] = sheetName;

		UnityWebRequest request = UnityWebRequest.Post(serviceURL, form);
		return request;
	}

	public static List<List<string>> ParseResult(UnityWebRequest completedRequest) {
		UnityEngine.Assertions.Assert(completedRequest.isDone);
		return ParseTable(completedRequest.downloadHandler.text);
	}

	/// <summary>
	/// This is a very simple parser that only handles a 2D array (i.e.,
	/// array of arrays) in JSON format.  The individual cell elements
	/// will be strings or numbers, but are always returned here as strings.
	/// </summary>
	/// <param name="jsonData"></param>
	/// <returns></returns>
	public static List<List<string>> ParseTable(string jsonData) {
		List<List<string>> result = new List<List<string>>();
		int pos = 0;
		// Check for the outer brackets; then parse each inner bracket as an array.
		QA.Assert(NextAfterWhitespace(jsonData, ref pos) == '[');    // start of table
		while (true) {
			SkipWhitespace(jsonData, ref pos);
			if (jsonData[pos] == '[') result.Add(ParseRow(jsonData, ref pos));
			else break;
			SkipWhitespace(jsonData, ref pos);
			if (jsonData[pos] != ',') break;
			pos++;
		}
		QA.Assert(NextAfterWhitespace(jsonData, ref pos) == ']');    // end of table
		return result;
	}

	static List<string> ParseRow(string jsonData, ref int pos) {
		List<string> result = new List<string>();

		// Check for the open bracket; then parse inner stuff, until we get to a close bracket
		int beforePos = pos;
		QA.Assert(NextAfterWhitespace(jsonData, ref pos) == '[');
		while (jsonData[pos] != ']') {
			result.Add(ParseOneItem(jsonData, ref pos));
			SkipWhitespace(jsonData, ref pos);
			if (jsonData[pos] == ']') break;
			QA.Assert(jsonData[pos] == ',');
			pos++;
			SkipWhitespace(jsonData, ref pos);
		}

		QA.Assert(jsonData[pos] == ']');
		pos++;
		return result;
	}

	static string ParseOneItem(string jsonData, ref int pos) {
		SkipWhitespace(jsonData, ref pos);
		int startPos = pos;
		if (jsonData[pos] == '"') {
			// Parse a string, until an (unescaped) end quote
			startPos++;
			pos++;
			while (pos < jsonData.Length) {
				if (jsonData[pos] == '\\') pos++;
				else if (jsonData[pos] == '"') {
					string result = jsonData.Substring(startPos, pos - startPos);
					result = result.Replace("\\\"", "\"");
					pos++;    // skip close quote
					return result;
				}
				pos++;
			}
		}
		// Parse a number, until we get to a comma
		while (pos < jsonData.Length) {
			if (jsonData[pos] == ',') {
				return jsonData.Substring(startPos, pos - startPos);
			}
			pos++;
		}
		return null;
	}

	static void SkipWhitespace(string data, ref int pos) {
		while (pos < data.Length) {
			char c = data[pos];
			if (c > ' ') return;
			pos++;
		}
	}

	static char NextAfterWhitespace(string data, ref int pos) {
		while (pos < data.Length) {
			char c = data[pos];
			pos++;
			if (c > ' ') return c;
		}
		return (char)0;
	}*/
}

/*
    // ************** Global vars/consts **************

var OPEN_SS_DATA;
var PASSWORD = "yourPassword"


	// ************** Entry Points (Get/Post) **************

	function doPost(e)
{
	return Process(e);
}

	function doGet(e)
{
	return Process(e);
}

	// ************** Initialization functions **************

	function Process(e)
{
	// Password Check.
	if (e.parameters.pass != PASSWORD)
		return ContentService.createTextOutput("Invalid password");

	// Useful for service status quick testing.
	if (e.parameters.test != null)
		return ContentService.createTextOutput(JSON.stringify(e));

	// Load required stuff.
	OpenSheet(e);

	// Parse the request.
	var result = ParseGetRawData(e);

	// Answer the call.
	return ContentService.createTextOutput(result);
}

	function OpenSheet(e)
{
	if (e.parameters.ssid == null)
		return "Missing ssid parameter";

	var SS_DATA = e.parameters.ssid.toString();

	// Open worksheet.
	OPEN_SS_DATA = SpreadsheetApp.openById(SS_DATA);
}

	function ParseGetRawData(e)
{
	if (e.parameters.sheet == null)
		return "Missing sheet parameter";

	var sheet = OPEN_SS_DATA.getSheetByName(e.parameters.sheet.toString());
	var tableData = sheet.getDataRange().getValues();

	return JSON.stringify(tableData);
}
	*/
