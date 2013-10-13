#import "ImageSync.h"
#import <AssetsLibrary/ALAsset.h>
#import <AssetsLibrary/ALAssetRepresentation.h>
#import "ASIHTTPRequest.h"
#import "ASINetworkQueue.h"
#import "ASIFormDataRequest.h"
#import "MPNotificationView.h"

@implementation ImageSync;


-(CDVPlugin*) initWithWebView:(UIWebView*)theWebView
{
    self = (ImageSync*)[super initWithWebView:theWebView];
    if (self)
	{
        //NSLog(@"ImageSync Initialized!");
    }
    
    return self;
}


+ (ALAssetsLibrary *)defaultAssetsLibrary {
    static dispatch_once_t pred = 0;
    static ALAssetsLibrary *library = nil;
    dispatch_once(&pred, ^{
        library = [[ALAssetsLibrary alloc] init];
    });
    return library;
}


- (void)uploadPictures:(CDVInvokedUrlCommand*)command {
    
    
    UIImage *picServ = [UIImage imageNamed:@"www/img/Icon.png"];
    [MPNotificationView notifyWithText:@"Starting Uploads" detail:@"Checking for new items" image:picServ andDuration:1];
    
    dispatch_group_t loadingGroup = dispatch_group_create();
    NSMutableArray * albums = [[NSMutableArray array] init];
    [self loadPreviousUploads];

    void (^assetEnumerator)(ALAsset *, NSUInteger, BOOL *) = ^(ALAsset *asset, NSUInteger index, BOOL *stop) {
        if(index != NSNotFound) {
            NSString *uniqueName = [self getUniqueName:asset];
            if ([uploads containsObject: uniqueName] == NO) {
                [assets addObject:asset];
            }
        } else {
            dispatch_group_leave(loadingGroup);
        }
    };
    
    
    void (^assetGroupEnumerator)(ALAssetsGroup *, BOOL *) =  ^(ALAssetsGroup *group, BOOL *stop) {
        if(group != nil) {
            [albums addObject: group];
        } else {
            for (ALAssetsGroup * album in albums) {
                dispatch_group_enter(loadingGroup);
                [album enumerateAssetsUsingBlock: assetEnumerator];
            }
            dispatch_group_notify(loadingGroup, dispatch_get_main_queue(), ^{
                if ([assets count] == 0 ) {
                    UIImage *picServ = [UIImage imageNamed:@"www/img/Icon.png"];
                    [MPNotificationView notifyWithText:@"Nothing to upload" detail:@"All photos uploaded" image:picServ andDuration:1];
                } else {
                    [self beginQueue];
                }
            });
        }
    };
    
    ALAssetsLibrary * library = [ImageSync defaultAssetsLibrary];
    [ library enumerateGroupsWithTypes:ALAssetsGroupSavedPhotos | ALAssetsGroupAlbum
                           usingBlock:assetGroupEnumerator
                         failureBlock: ^(NSError *error) {
                             NSLog(@"Failed.");
                         }];
    
    
}

- (void) beginQueue {
    
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    NSString *baseURL = [NSString stringWithFormat:@"http://%@:%@/upload/?key=%@",
                           [defaults objectForKey:@"ip"],
                           [defaults objectForKey:@"uploadport"],
                           [defaults objectForKey:@"password"]];

    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,  NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths objectAtIndex:0];
    
    dispatch_queue_t queue = dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0ul);
    dispatch_async(queue, ^{
        int i = 0;
        for (ALAsset *asset in assets) {
            i++;
            ALAssetRepresentation *representation = [asset defaultRepresentation];
            NSString *uniqueName = [self getUniqueName:asset];
            NSString* filename = [documentsDirectory stringByAppendingPathComponent:uniqueName];
            
            //update UI
            dispatch_sync(dispatch_get_main_queue(), ^{
                CGImageRef iref = [asset thumbnail];
                if (iref) {
                    float percentage = (100 * i)/[assets count];
                    
                    UIImage *theThumbnail = [UIImage imageWithCGImage:iref];
                    NSString *uploadText = [NSString stringWithFormat:@"Item %d of %d (%.0f%% Complete)",
                                            i,
                                            [assets count],
                                            percentage];
                    NSString *uploadDesc = [NSString stringWithFormat:@"Uploading: %@", [representation filename]];
                    
                    
                    [MPNotificationView notifyWithText:uploadDesc detail:uploadText image:theThumbnail andDuration:0.5];
                    
                }
            });
            
            //copy media to documents directory
            [[NSFileManager defaultManager] createFileAtPath:uniqueName contents:nil attributes:nil];
            NSOutputStream *outPutStream = [NSOutputStream outputStreamToFileAtPath:filename append:YES];
            [outPutStream open];

            long long offset = 0;
            long long bytesRead = 0;
            NSError *error;
            uint8_t * buffer = malloc(131072);
            while (offset < [representation size] && [outPutStream hasSpaceAvailable]) {
                bytesRead = [representation getBytes:buffer fromOffset:offset length:131072 error:&error];
                [outPutStream write:buffer maxLength:bytesRead];
                offset = offset+bytesRead;
            }
            [outPutStream close];
            free(buffer);
            
            //upload it to webserver
            NSString *uploadURL = [baseURL stringByAppendingString:@"&FileName="];
            uploadURL = [uploadURL stringByAppendingString:uniqueName];
            uploadURL = [uploadURL stringByAppendingString:@"&Device="];
            uploadURL = [uploadURL stringByAppendingString:[self getDeviceName]];
            
            ASIFormDataRequest *request = [ASIFormDataRequest requestWithURL:[NSURL URLWithString:uploadURL]];
            NSData *imageData = [[NSFileManager defaultManager] contentsAtPath:filename];
            [request setData:imageData withFileName:uniqueName andContentType:@"image/jpeg" forKey:@"image"];
            [request startSynchronous];


            NSError *uploadError = [request error];
            if (!uploadError) {
                NSString *response = [request responseString];
                if ([response isEqualToString:@"Success"]) {
                    [uploads addObject:uniqueName];
                    [self saveUploads];
                }
            }
            //delete the local cached copy
            [[NSFileManager defaultManager] removeItemAtPath:filename error:nil];
            
        }
        
        dispatch_sync(dispatch_get_main_queue(), ^{
            UIImage *picServ = [UIImage imageNamed:@"www/img/Icon.png"];
            [MPNotificationView notifyWithText:@"Complete" detail:@"All photos uploaded" image:picServ andDuration:1.0];
        
            NSString *refreshURL = [NSString stringWithFormat:@"http://%@:%@/cacheRefresh/?key=%@",
                                    [defaults objectForKey:@"ip"],
                                    [defaults objectForKey:@"uploadport"],
                                    [defaults objectForKey:@"password"]];
            NSURL *url = [NSURL URLWithString:refreshURL];
            
            ASIHTTPRequest *request = [ASIHTTPRequest requestWithURL:url];
            [request startSynchronous];
            NSError *error = [request error];
            if (!error) {
                //refresh triggered
                // NSString *response = [request responseString];
            }
            
        });
        
        
        


    });

}
-(void) saveUploads{
    NSArray *paths = NSSearchPathForDirectoriesInDomains
    (NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths objectAtIndex:0];
    NSString *fileName = [NSString stringWithFormat:@"%@/upload.dat",
                          documentsDirectory];
    NSString *content = [uploads componentsJoinedByString:@"||"];
    [content writeToFile:fileName
              atomically:NO
                encoding:NSStringEncodingConversionAllowLossy
                   error:nil];
    
}
-(void) loadPreviousUploads {
    [uploads removeAllObjects];
    uploads = [[NSMutableArray array] init];
    [assets removeAllObjects];
    assets = [[NSMutableArray array] init];
    
    NSArray *paths = NSSearchPathForDirectoriesInDomains
    (NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths objectAtIndex:0];
    
    NSString *fileName = [NSString stringWithFormat:@"%@/upload.dat",
                          documentsDirectory];
    
    BOOL fileExists = [[NSFileManager defaultManager] fileExistsAtPath:fileName];
    
    if ( fileExists ) {
        NSString* content = [NSString stringWithContentsOfFile:fileName
                                                      encoding:NSUTF8StringEncoding
                                                         error:NULL];
        
        uploads = [[content componentsSeparatedByString:@"||"] mutableCopy];
    }
}

- (NSString *)getUniqueName:(ALAsset*)asset {
    ALAssetRepresentation *representation = [asset defaultRepresentation];
    NSDate *date = [asset valueForProperty:ALAssetPropertyDate];
    NSDateFormatter *dateFormatter = [[NSDateFormatter alloc] init];
    [dateFormatter setLocale:[NSLocale currentLocale]];
    [dateFormatter setDateFormat:@"yyyy.MM.dd-hhmmssAAA"];
    
    
    NSString *uniqueName = [NSString stringWithFormat:@"%@_%@", [dateFormatter stringFromDate:date], [representation filename]];
    return uniqueName;
}

- (NSString *)getDeviceName {
    NSString *deviceName = [[UIDevice currentDevice] name];
    NSCharacterSet *charactersToRemove = [[ NSCharacterSet alphanumericCharacterSet ] invertedSet ];
    deviceName = [deviceName stringByTrimmingCharactersInSet:charactersToRemove ];
    deviceName = [deviceName stringByReplacingOccurrencesOfString:@" " withString:@"_"];
    return deviceName;
}


@end
