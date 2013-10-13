
#import <Cordova/CDVPlugin.h>
#import <UIKit/UIKit.h>

@class ASINetworkQueue;

@interface ImageSync: CDVPlugin {
    NSMutableArray *assets;
    NSMutableArray *uploads;
}


- (void)uploadPictures:(CDVInvokedUrlCommand*)command;

@end

