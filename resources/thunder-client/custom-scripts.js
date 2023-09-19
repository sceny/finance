/// <reference path="./tc-types.d.ts" />

const CryptoJS = require('crypto-js');

async function getCoinbaseAccessSign(input, method, requestPath, body) {
    const timestamp = Math.floor(Date.now() / 1000).toString();
    console.log('method         ➡️ ' + method);
    console.log('requestPath    ➡️ ' + requestPath);
    console.log('body           ➡️ ' + body);
    const accessSign =
        (timestamp ?? '') +
        (method ?? '') +
        `/api/v3/brokerage${requestPath ?? '' ?? ''}` +
        (body ?? '');
    console.log('accessSign     ➡️ ' + accessSign);

    const apiSecret = tc.getVar('coinbase/apiSecret');
    const hash = CryptoJS.HmacSHA256(accessSign, apiSecret);

    return hash.toString();
}

module.exports = [getCoinbaseAccessSign];
