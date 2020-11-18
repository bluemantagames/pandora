using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Runtime.InteropServices;
using System;
using AOT;

// THX https://gist.github.com/BastianBlokland/bbc02a407b05beaf3f55ead3dd10f808
// <3 <3 <3

public class CustomGameCenter
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void IdentityVerificationSignatureCallback(
        string publicKeyUrl,
        IntPtr signaturePointer, int signatureLength,
        IntPtr saltPointer, int saltLength,
        ulong timestamp,
        string error);

    [DllImport("__Internal")]
    private static extern void generateIdentityVerificationSignature(
    [MarshalAs(UnmanagedType.FunctionPtr)] IdentityVerificationSignatureCallback callback);

    [MonoPInvokeCallback(typeof(IdentityVerificationSignatureCallback))]
    private static void OnIdentityVerificationSignatureGenerated(
        string publicKeyUrl,
        IntPtr signaturePointer, int signatureLength,
        IntPtr saltPointer, int saltLength,
        ulong timestamp,
        string error)
    {
        // Create a managed array for the signature
        var signature = new byte[signatureLength];
        Marshal.Copy(signaturePointer, signature, 0, signatureLength);

        // Create a managed array for the salt
        var salt = new byte[saltLength];
        Marshal.Copy(saltPointer, salt, 0, saltLength);

        UnityEngine.Debug.Log($"publicKeyUrl: {publicKeyUrl}");
        UnityEngine.Debug.Log($"signature: {signature.Length}");
        UnityEngine.Debug.Log($"salt: {salt.Length}");
        UnityEngine.Debug.Log($"timestamp: {timestamp}");
        UnityEngine.Debug.Log($"error: {error}");
    }
}
