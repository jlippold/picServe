//
//  ImageView.h
//  picServe
//
//  Created by JED LIPPOLD on 8/17/13.
//
//
#import <Cordova/CDVViewController.h>
#import <Cordova/CDVPlugin.h>
#import <UIKit/UIKit.h>
#import <CoreData/CoreData.h>
#import "ProgressHud.h"


@interface ImageView : CDVPlugin <UIWebViewDelegate>{
    CGRect _originalWebViewFrame;
	NSMutableArray* _imageViewData;
    NSUInteger _imageIndex;
}


@property (nonatomic, strong) IBOutlet UIImageView *imageView;
@property (nonatomic, strong) IBOutlet UIScrollView *scrollView;

@property (nonatomic, strong) IBOutlet UINavigationBar *navBar;
@property (nonatomic, strong) IBOutlet UINavigationItem *navTitle;
@property (nonatomic, strong) IBOutlet UIBarButtonItem *closeButton;
@property (nonatomic, strong) IBOutlet UIBarButtonItem *actionButton;

@property (nonatomic, strong) IBOutlet UIToolbar *toolbar;
@property (nonatomic, strong) IBOutlet UIBarButtonItem *deleteButton;
@property (nonatomic, strong) IBOutlet UIBarButtonItem *downloadButton;

@property (nonatomic, strong) IBOutlet UIBarButtonItem *prevButton;
@property (nonatomic, strong) IBOutlet UIBarButtonItem *nextButton;

@property (nonatomic, strong) IBOutlet UIActivityIndicatorView *spinner;
@property (nonatomic, unsafe_unretained) NSTimer *timer;
@property (nonatomic, unsafe_unretained) MBProgressHUD* progressHUD;


- (void)createImageView:(NSArray*)arguments withDict:(NSDictionary*)options;
- (void)showImageView:(NSArray*)arguments withDict:(NSDictionary*)options;
- (void)playIt:(NSString*)videoLocation;

@end

