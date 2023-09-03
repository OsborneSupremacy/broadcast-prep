// using System.Net.Http;
// using Broadcast.Prep.Data;
// using BroadCast.Prep.Models;
// using OsborneSupremacy.Extensions.AspNet;
// using Spectre.Console;

// namespace BroadCast.Prep.Service;

// public static class SquarespaceUploadService
// {
//     public static Outcome<bool> Process(Settings settings)
//     {
//         try
//         {


//             return new Outcome<bool>(true);
//         }
//         catch (Exception ex)
//         {
//             return new Outcome<bool>(ex);
//         }
//     }

//     private static Outcome<bool> AddSquarespaceBlogPost(SermonData sermonData)
//     {
//         var _httpClient = HttpClientFactory().Create();

//         try
//         {
//             _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
//             _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

//             var requestBody = new
//             {
//                 title,
//                 content,
//                 // Add more fields as needed (e.g., categories, tags, etc.)
//             };

//             var contentToSend = new StringContent(
//                 JsonSerializer.Serialize(requestBody),
//                 Encoding.UTF8,
//                 "application/json"
//             );

//             var response = await _httpClient.PostAsync($"{ApiBaseUrl}/blog/posts", contentToSend);

//             if (response.IsSuccessStatusCode)
//             {
//                 var responseContent = await response.Content.ReadAsStringAsync();
//                 // Parse the response JSON and extract relevant information
//                 // For example: return the created post's ID or other details
//                 return responseContent;
//             }
//             else
//             {
//                 var errorContent = await response.Content.ReadAsStringAsync();
//                 // Handle error or throw an exception
//                 throw new Exception($"Failed to add blog post: {errorContent}");
//             }
//         }
//         }
//         catch (Exception ex)
//         {
//             return new Outcome<bool>(ex);
//         }
//     }
// }