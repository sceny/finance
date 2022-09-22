using System;
using System.Globalization;

namespace WebApp.Domain.Parsers.Qif;

public class QifAccountItemFiller
{
    public ValueParser ValueParser { get; }

    public QifAccountItemFiller(ValueParser valueParser) =>
        ValueParser = valueParser ?? throw new ArgumentNullException(nameof(valueParser));

    public void Fill(AccountItem item, string code, string value)
    {
        switch (code)
        {
            case "D":
                FillDate(item, value);
                break;
            case "T":
                FillAmount(item, value);
                break;
            case "U":
                // Seems identical to T field (amount of item.) Both T and U are present in QIF files exported from Quicken 2015.	All	U-1,234.50
                break;
            case "M":
                // Memo—any text you want to record about the item.	All	Mgasoline for my car
                break;
            case "C":
                // Cleared status. Values are blank (unreconciled/not cleared), "*" or "c" (cleared) and "X" or "R" (reconciled).	All	CR
                break;
            case "N":
                // Number of the check. Can also be "Deposit", "Transfer", "Print", "ATM", "EFT".	Banking, Splits	N1001
                break;
            case "P":
                FillPayee(item, value);
                break;
            case "A":
                // Address of Payee. Up to 5 address lines are allowed. A 6th address line is a message that prints on the check. 1st line is normally the same as the Payee line—the name of the Payee.	Banking, Splits	A101 Main St.
                break;
            case "L":
                // Category or Transfer and (optionally) Class. The literal values are those defined in the Quicken Category list. SubCategories can be indicated by a colon (":") followed by the subcategory literal. If the Quicken file uses Classes, this can be indicated by a slash ("/") followed by the class literal. For Investments, MiscIncX or MiscExpX actions, Category/class or transfer/class. (40 characters maximum)	Banking, Splits	LFuel:car
                break;
            case "F":
                // Flag this transaction as a reimbursable business expense.	Banking	F???
                break;
            case "S":
                // Split category. Same format as L (Categorization) field. (40 characters maximum)	Splits	Sgas from Esso
                break;
            case "E":
                // Split memo—any text to go with this split item.	Splits	Ework trips
                break;
            case "$":
                // Amount for this split of the item. Same format as T field.	Splits	$1,000.50
                break;
            case "%":
                // Percent. Optional—used if splits are done by percentage.	Splits	%50
                break;
            // case "N":
            //     // Investment Action (Buy, Sell, etc.).	Investment	NBuy
            //     break;
            case "Y":
                // Security name.	Investment	YIDS Federal Income
                break;
            case "I":
                // Price.	Investment	I5.125
                break;
            case "Q":
                // Quantity of shares (or split ratio, if Action is StkSplit).	Investment	Q4,896.201
                break;
            case "O":
                // Commission cost (generally found in stock trades)	Investment	O14.95
                break;
            // case "$":
            //     // Amount transferred, if cash is moved between accounts	Investment	$25,000.00
            //     break;
            case "B":
                // Budgeted amount - may be repeated many times for monthly budgets.	Categories	B85.00
                break;
            case "X":
                // Extended data for Quicken Business. Followed by a second character subcode (see below) followed by content data.	Invoices	XI3
                break;
            case "XA":
                // Ship-to address	Invoices	XAATTN: Receiving
                break;
            case "XI":
                // Invoice transaction type: 1 for invoice, 3 for payment	Invoices	XI1
                break;
            case "XE":
                // Invoice due date	Invoices	XE6/17' 2
                break;
            case "XC":
                // Tax account	Invoices	XC[*Sales Tax*]
                break;
            case "XR":
                // Tax rate	Invoices	XR7.70
                break;
            case "XT":
                // Tax amount	Invoices	XT15.40
                break;
            case "XS":
                // Line item description	Invoices	XSRed shoes
                break;
            case "XN":
                // Line item category name	Invoices	XNSHOES
                break;
            case "X#":
                // Line item quantity	Invoices	X#1
                break;
            case "X$":
                // Line item price per unit (multiply by X# for line item amount)	Invoices	X$150.00
                break;
            case "XF":
                // Line item taxable flag	Invoices	XFT
                break;
        }
    }

    /// <summary>
    /// Date.
    /// <para>Leading zeroes on month and day can be skipped. Year can be either 4 digits or 2 digits or '6 (=2006).</para>
    /// <para>D25 December 2006</para>
    /// </summary>
    private void FillDate(AccountItem item, ReadOnlySpan<char> value) =>
        item.Date = ValueParser.TryParseDate(value, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Unable to parse date: {value.ToString()}");

    /// <summary>
    /// <para>Amount of the item.</para>
    /// <para>For payments, a leading minus sign is required. For deposits, either no sign or a leading plus sign is accepted. Do not include currency symbols ($, £, ¥, etc.). Comma separators between thousands are allowed.</para>
    /// <para>T-1,234.50</para>
    private void FillAmount(AccountItem item, ReadOnlySpan<char> value) =>
        item.Amount = ValueParser.TryParseNumber(value, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Unable to parse number: {value.ToString()}");

    /// <summary>
    /// Payee. Or a description for deposits, transfers, etc.	Banking, Investment	PStandard Oil, Inc.
    /// </summary>
    private static void FillPayee(AccountItem item, string value)
    {
        item.Payee = value;
        // TODO: Allow swiping memo and payee
    }
}