#import "ImageCollection.h"
#import "Base64.h"
#import "CVCell.h"
#import "CVHeader.h"
//#import <QuartzCore/QuartzCore.h>
#import "SSZipArchive.h"
#import "ASIHTTPRequest.h"
#import "ASINetworkQueue.h"
#import "UIBAlertView.h"

@implementation ImageCollection;
@synthesize collectionView = _collectionView;
@synthesize searchBar = _searchBar;
@synthesize searchController = _searchController;
@synthesize managedObjectContext = _managedObjectContext;
@synthesize navBar = _navbar;
@synthesize networkQueue;

static dispatch_queue_t concurrentQueue = NULL;
static dispatch_queue_t imageQueue = NULL;

-(CDVPlugin*) initWithWebView:(UIWebView*)theWebView
{
    self = (ImageCollection*)[super initWithWebView:theWebView];
    
    [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
    [[NSNotificationCenter defaultCenter]
     addObserver:self selector:@selector(orientationChanged:)
     name:UIDeviceOrientationDidChangeNotification
     object:[UIDevice currentDevice]];
    
    return self;
}


- (void) orientationChanged:(NSNotification *)note
{
    [self resizeView];
}

- (void)resizeView {
    
    _navbar.frame = CGRectMake(0, 0, self.webView.superview.bounds.size.width, _navbar.frame.size.height);
    _collectionView.frame = CGRectMake(0, _collectionView.frame.origin.y, self.webView.superview.bounds.size.width, self.webView.superview.frame.size.height - _navbar.frame.size.height);
    [_collectionView.collectionViewLayout invalidateLayout];

}

#pragma mark - JS interface methods

- (void)createImageView:(NSArray*)arguments withDict:(NSDictionary*)options
{

    //setup a network queue
    [self setNetworkQueue:[ASINetworkQueue queue]];
    [[self networkQueue] setDelegate:self];
    [[self networkQueue] setRequestDidFinishSelector:@selector(requestFinished:)];
    [[self networkQueue] setRequestDidFailSelector:@selector(requestFailed:)];
    [[self networkQueue] setQueueDidFinishSelector:@selector(queueFinished:)];
    [[self networkQueue] setShouldCancelAllRequestsOnFailure:NO];
    [self networkQueue].maxConcurrentOperationCount = 2;

    CGRect navBarFrame = CGRectMake(0, 0, self.webView.superview.bounds.size.width, 44.0);
    if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 7.0) {
        navBarFrame.size.height = 64;
    }
    _navbar = [[UINavigationBar alloc] initWithFrame:navBarFrame];
    
    _navbar.barStyle = UIBarStyleDefault;
    //UIImage *backgroundImage = [UIImage imageNamed:@"www/img/navBar.png"];
    //[_navbar setBackgroundImage:backgroundImage forBarMetrics:0];
    
    UINavigationItem *navItem = [UINavigationItem alloc];
    NSString *navTitle = [options objectForKey:@"navTitle"];
    navItem.title = navTitle;
    
    [_navbar pushNavigationItem:navItem animated:false];
    [self.webView.superview addSubview:_navbar];
    [_navbar setHidden:YES];
    
    if ( [[options objectForKey:@"showBackButton"] boolValue] == true) {

        NSString *backArrowString = @"\U000025C0\U0000FE0E"; //BLACK LEFT-POINTING TRIANGLE PLUS VARIATION SELECTOR
        backArrowString = [options objectForKey:@"backButtonText"];
        UIBarButtonItem *backBarButtonItem = [[UIBarButtonItem alloc] initWithTitle:backArrowString style:UIBarButtonItemStylePlain target:self action:@selector(onBackButtonPress:)];
        navItem.leftBarButtonItem = backBarButtonItem;
/*
        
        UIButton *settingsView = [[UIButton alloc] initWithFrame:CGRectMake(0.0, 100.0, 60.0, 30.0)];
        [settingsView addTarget:self action:@selector(onBackButtonPress:) forControlEvents:UIControlEventTouchUpInside];
        UIImage *backButtonImage = [[UIImage imageNamed:@"www/img/UINavigationBarBlackOpaqueBack.png"] resizableImageWithCapInsets:UIEdgeInsetsMake(0, 13, 0, 6)];

        [settingsView setBackgroundImage:backButtonImage forState:UIControlStateNormal];
        [settingsView setTitle:[options objectForKey:@"backButtonText"] forState:UIControlStateNormal];
        settingsView.titleLabel.font = [UIFont boldSystemFontOfSize:15.0f];
        settingsView.titleLabel.adjustsFontSizeToFitWidth = TRUE;
        [settingsView.layer setMasksToBounds:YES];
        [settingsView setTitleEdgeInsets:UIEdgeInsetsMake(0.0, 10.0, 0.0, 5.0)];

        
        UIBarButtonItem *settingsButton = [[UIBarButtonItem alloc] initWithCustomView:settingsView];
        [navItem setLeftBarButtonItem:settingsButton];
*/
    }
    
    if ( [[options objectForKey:@"showRightButton"] boolValue] == true) {
        UIBarButtonItem *rightButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemAction target:self action:@selector(onRightButtonPress:)];
         navItem.rightBarButtonItem = rightButton;
    } else {
        UIBarButtonItem *rightButtonEdit = [[UIBarButtonItem alloc] initWithTitle:@"Edit" style:UIBarButtonSystemItemDone target:self action:@selector(onEdit:)];
        navItem.rightBarButtonItem = rightButtonEdit;
    }
    
    

    //Download zip file
    if ([[options objectForKey:@"zipcache"] isEqualToString:@""] ) {
        
    } else {
        /*
        dispatch_queue_t queue = dispatch_get_global_queue(0,0);
        dispatch_async(queue, ^{
            NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,  NSUserDomainMask, YES);
            NSString *documentsDirectory = [paths objectAtIndex:0];
            NSString *cachedFile = [documentsDirectory stringByAppendingPathComponent:[NSString stringWithFormat:@"%@.zip", [options objectForKey:@"zipcache"] ]];
            
            NSString *url = [NSString stringWithFormat:@"%@&Path=%@", [options objectForKey:@"zipAddress"], [options objectForKey:@"zipcache"]];
            NSData *zipData = [NSData  dataWithContentsOfURL:[NSURL URLWithString:url]];
            [zipData writeToURL:[NSURL fileURLWithPath:cachedFile] options:0 error:nil];
            if ( [[NSFileManager defaultManager] fileExistsAtPath:cachedFile] ) {
                [SSZipArchive unzipFileAtPath:cachedFile toDestination:[paths objectAtIndex:0]];
                [[NSFileManager defaultManager] removeItemAtPath:cachedFile error:nil];
                //NSLog(@"unZIPPED!");
            }
        });
         */
    }

    
    _mainTableHeight = 200;
    
    UICollectionViewFlowLayout *flowLayout = [[UICollectionViewFlowLayout alloc] init];
    [flowLayout setScrollDirection:UICollectionViewScrollDirectionVertical];
    [flowLayout setItemSize:CGSizeMake(100, 100)];
    [flowLayout setMinimumInteritemSpacing:0.f];
    [flowLayout setMinimumLineSpacing:5];
    [flowLayout setSectionInset:UIEdgeInsetsMake(5, 5, 5, 5)];
    flowLayout.headerReferenceSize = CGSizeMake(0, 30);
    _originalWebViewFrame = self.webView.frame;
	
	CGRect myFrame;
	
	myFrame = CGRectMake(
                                _originalWebViewFrame.origin.x,
                                _originalWebViewFrame.origin.y + _navbar.frame.size.height,
                                _originalWebViewFrame.size.width,
                                _originalWebViewFrame.size.height - _navbar.frame.size.height
                                );
    
    self.webView.scrollView.scrollsToTop = NO;
    _collectionView.scrollsToTop = YES;

    _collectionView = [[UICollectionView alloc] initWithFrame:myFrame collectionViewLayout:flowLayout];

    _collectionView.backgroundColor = [UIColor colorWithRed:0 green:0.322 blue:0.478 alpha:1];
    [_collectionView setDataSource:self];
    [_collectionView setDelegate:self];
    [_collectionView setBounces:YES];
    [_collectionView registerClass:[CVCell class] forCellWithReuseIdentifier:@"cvCell"];
    [_collectionView setHidden:YES];
    
 
   [_collectionView registerClass:[HeaderView class] forSupplementaryViewOfKind:UICollectionElementKindSectionHeader withReuseIdentifier:@"HeaderView"];
    
    isEditing = NO;
    
	self.webView.superview.autoresizesSubviews = YES;
	[self.webView.superview addSubview:_collectionView];
    
}

- (CGSize)collectionView:(UICollectionView *)collectionView layout:(UICollectionViewLayout*)collectionViewLayout sizeForItemAtIndexPath:(NSIndexPath *)indexPath; {
    if ( UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad ) {
        //return CGSizeMake(160, 160);
        return CGSizeMake(220, 220);
    } else {
        return CGSizeMake(102, 102);
    }
}



- (void)scrollViewDidEndDragging:(UIScrollView *)scrollView willDecelerate:(BOOL)decelerate
{
    if (!decelerate) {
        [self loadImagesForOnscreenRows];
    } else {
        [self cancelTimer];
    }
}
- (void)scrollViewDidEndScrollingAnimation:(UIScrollView *)scrollView
{
    [self loadImagesForOnscreenRows];
}

- (void)scrollViewWillBeginDragging:(UIScrollView *)scrollView
{
    [self cancelTimer];
}

- (void)scrollViewDidEndDecelerating:(UIScrollView *)scrollView
{
   [self loadImagesForOnscreenRows];
}

- (void)scrollViewDidScroll:(UIScrollView *)scrollView
{
    [self cancelTimer];
}

- (void)scrollViewDidScrollToTop:(UIScrollView *)scrollView
{
    [self loadImagesForOnscreenRows];
}


- (void)setImageViewData:(NSArray*)arguments withDict:(NSDictionary*)options
{
    
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,  NSUserDomainMask, YES);
    NSString *documentsDirectory = [paths objectAtIndex:0];
    
	NSMutableArray *JSArray = [[arguments objectAtIndex:0] mutableCopy];
    /* translate JS array to CollectionView Array */
    NSString *loopedHeader = @"";
    NSMutableArray *tmpList = [[NSMutableArray alloc] init];
    _data = [[NSMutableArray alloc] init];
    for(int i = 0; i < [JSArray count]; i++ ) {
        NSString *thisHeader = [[JSArray objectAtIndex:i] valueForKey:@"sectionHeader"];
        if ( ![thisHeader isEqualToString:loopedHeader]) {
            if (![loopedHeader isEqualToString:@""]) {
                NSMutableArray *toCopy = [[NSMutableArray alloc] initWithArray: tmpList];
                [_data addObject:toCopy];
            }
            loopedHeader = thisHeader;
            [tmpList removeAllObjects];
            tmpList = [[NSMutableArray alloc] init];
        }
        
        //Add in other info about the item
        [[JSArray objectAtIndex:i] setObject:[NSString stringWithFormat:@"%d", i] forKey:@"JSIndex"];
        [[JSArray objectAtIndex:i] setObject:@"" forKey:@"Base64"];
        [[JSArray objectAtIndex:i] setObject:[documentsDirectory stringByAppendingPathComponent:[NSString stringWithFormat:@"%@.cache", [[JSArray objectAtIndex:i] valueForKey:@"cachePath"] ]] forKey:@"cachedFile"];
        
        [tmpList addObject:[JSArray objectAtIndex:i]];
    }
    
    if (![loopedHeader isEqualToString:@""]) {
        NSMutableArray *toCopy = [[NSMutableArray alloc] initWithArray: tmpList];
        [_data addObject:toCopy];
    }
	[_collectionView reloadData];
	
}

- (UICollectionViewCell *)collectionView:(UICollectionView *)cv cellForItemAtIndexPath:(NSIndexPath *)indexPath {
   

    CVCell *cell = (CVCell *)[cv dequeueReusableCellWithReuseIdentifier:@"cvCell" forIndexPath:indexPath];
    NSMutableArray *sectionData = [_data objectAtIndex:indexPath.section];

    NSDictionary *item = [sectionData objectAtIndex:indexPath.row];
    NSString *cachePath = [item valueForKey:@"cachePath"];
    NSString *url = [item valueForKey:@"image"];
    
    [cell.titleLabel setText:[item valueForKey:@"name"]];
    //cell.titleLabel.shadowColor = [UIColor blackColor];
    //cell.titleLabel.shadowOffset = CGSizeMake(0,-0.4);
    
    if (isEditing) {
        [cell.deleteButton setHidden:NO];
    }else {
        [cell.deleteButton setHidden:YES];
    }
    
    [cell.videoOverlay setHidden:YES];
    
    if (![url hasPrefix:@"http"]) {
        [cell.imageView.layer setBorderColor: [[UIColor clearColor] CGColor]];
        [cell.imageView.layer setBorderWidth: 0.0];
        cell.imageView.image = [UIImage imageNamed:[item valueForKey:@"image"]];
        [cell.deleteButton setHidden:YES];
    } else {
        
        [cell.imageView.layer setBorderColor: [[UIColor whiteColor] CGColor]];
        [cell.imageView.layer setBorderWidth: 2.0];
        
        //check if the cached version exists
        if ([[item valueForKey:@"Base64"] isEqualToString:@""]) {
            BOOL fileExists = [[NSFileManager defaultManager] fileExistsAtPath:[item valueForKey:@"cachedFile"]];
            if (fileExists) {
                NSString *content = [NSString stringWithContentsOfFile:[item valueForKey:@"cachedFile"] encoding:NSUTF8StringEncoding error:nil];
                [item setValue:content forKey:@"Base64"];
                NSData *imageData = [NSData dataWithContentsOfURL:[NSURL URLWithString:content]];
                cell.imageView.image = [UIImage imageWithData:imageData];
                if ([cachePath hasSuffix:@".mov"]) {
                    [cell.videoOverlay setHidden:NO];
                }
            } else {
                if ([cachePath hasSuffix:@".mov"]) {
                    cell.imageView.image = [UIImage imageNamed:@"placeholderVideo.png"];
                } else {
                    cell.imageView.image = [UIImage imageNamed:@"placeholder.png"];
                }
            }
        } else {
            NSData *imageData = [NSData dataWithContentsOfURL:[NSURL URLWithString:[item valueForKey:@"Base64"]]];
            cell.imageView.image = [UIImage imageWithData:imageData];
            if ([cachePath hasSuffix:@".mov"]) {
                [cell.videoOverlay setHidden:NO];
            }
        }

        
    }
    
    [cell.deleteButton addTarget:self action:@selector(onDelete:event:) forControlEvents:UIControlEventTouchUpInside];
    
    return cell;
    
}

NSTimer *timer;
- (void) loadImagesForOnscreenRows {
    [self cancelTimer];
    timer = [NSTimer scheduledTimerWithTimeInterval:0.5 target:self selector:@selector(timerComplete:) userInfo:nil repeats:NO];
}

- (void) cancelTimer {
    [timer invalidate];
    timer = nil;
    //[[self networkQueue] cancelAllOperations];
}

- (void)timerComplete:(id)sender
{
    
    NSArray *ips = _collectionView.indexPathsForVisibleItems;
    NSMutableArray *items = [ips mutableCopy];
    
    [[self networkQueue] cancelAllOperations];
    
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
        
        //Pad more indexpaths to the end
        NSInteger lastRow = 0;
        NSInteger lastSection = 0;
        for (NSIndexPath *indexPath in items) {
            if (indexPath.row > lastRow) {
                lastRow = indexPath.row;
                lastSection = indexPath.section;
            }
        }
        for (int i = 0; i <= 20; i++) {
            NSInteger newRow = lastRow + i;
            if (newRow < [[_data objectAtIndex:lastSection] count]) {
                NSIndexPath *myIP = [NSIndexPath indexPathForRow:newRow inSection:lastSection] ;
                [items addObject:myIP];
            }
        }
        
        //loop on screen IP's and padding
        for (NSIndexPath *indexPath in items) {
            
            NSDictionary *item =[[_data objectAtIndex:indexPath.section] objectAtIndex:indexPath.row];
            NSString *url = [item valueForKey:@"image"];

            if ([url hasPrefix:@"http"] && [[item valueForKey:@"Base64"] isEqualToString:@""]) {
                
                int section = indexPath.section;
                int row = indexPath.row;
                
                BOOL needsKickStart = NO;
                if ([[self networkQueue] requestsCount] == 0) {
                    needsKickStart = YES;
                }
                
                ASIHTTPRequest *request = [ASIHTTPRequest requestWithURL:[[NSURL alloc] initWithString:url]];
                NSDictionary *reqInfo = [NSDictionary dictionaryWithObjectsAndKeys:
                                         [NSString stringWithFormat:@"%d",row], @"row",
                                         [NSString stringWithFormat:@"%d",section], @"section",
                                         [item valueForKey:@"cachedFile"], @"cachedFile",
                                         nil];
                
                [request setUserInfo:reqInfo];
                [[self networkQueue] addOperation:request];
                
                if (needsKickStart) {
                    [[self networkQueue] go];
                }
                
            }
            
        }
    });

}

- (void)requestFinished:(ASIHTTPRequest *)request
{
    dispatch_async(dispatch_get_global_queue( DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void){
        NSString *row = [[request userInfo] valueForKey:@"row"] ;
        NSString *section = [[request userInfo] valueForKey:@"section"];
        NSString *cachedFile = [[request userInfo] valueForKey:@"cachedFile"];
        NSData *data = UIImagePNGRepresentation([UIImage imageWithData:[request responseData]]);
        NSString *strEncoded = [Base64 encode:data];
        
        if (![strEncoded isEqualToString:@""]) {
            strEncoded = [@"data:image/jpg;base64," stringByAppendingString:strEncoded];
            [strEncoded writeToFile:cachedFile atomically:YES encoding:NSUTF8StringEncoding error:nil];
            NSIndexPath *IP = [NSIndexPath indexPathForRow:[row intValue] inSection:[section intValue]];
            NSDictionary *item =[[_data objectAtIndex:[section intValue]] objectAtIndex:[row intValue]];
            [item setValue:strEncoded forKey:@"Base64"];
            dispatch_async(dispatch_get_main_queue(), ^(void){
                if ( [[_collectionView indexPathsForVisibleItems] containsObject:IP] ) {
                    [_collectionView reloadItemsAtIndexPaths:@[IP]];
                }
            });
            
        }
    });

}

- (void)collectionView:(UICollectionView *)collectionView didSelectItemAtIndexPath:(NSIndexPath *)indexPath {
    [[self networkQueue] cancelAllOperations];
    if (isEditing) {
        return;
    }
    
	NSDictionary *item =[[_data objectAtIndex:indexPath.section] objectAtIndex:indexPath.row];
    
    NSString * jsCallBack = [NSString stringWithFormat:@"window.plugins.ImageCollection._onImageViewRowSelect(%@);", [item valueForKey:@"JSIndex"]];
    [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
}

- (UICollectionReusableView *)collectionView:(UICollectionView *)collectionView viewForSupplementaryElementOfKind:(NSString *)kind atIndexPath:(NSIndexPath *)indexPath
{

    HeaderView *header = [collectionView dequeueReusableSupplementaryViewOfKind:UICollectionElementKindSectionHeader withReuseIdentifier:@"HeaderView" forIndexPath:indexPath];
    
    UILabel *myLabel = (UILabel *)[header viewWithTag:1];
    if (!myLabel) {
        myLabel = [[UILabel alloc]initWithFrame:CGRectMake(0, 0, [_collectionView bounds].size.width, 26)];
        myLabel.tag = 1;
        [myLabel setBackgroundColor:[[UIColor blackColor] colorWithAlphaComponent:0.5f]];
        [myLabel setFont:[UIFont boldSystemFontOfSize:18.0f]];
        myLabel.textColor = [UIColor whiteColor];
        myLabel.shadowColor = [UIColor blackColor];
        myLabel.shadowOffset = CGSizeMake(0,1);
        [myLabel setOpaque:YES];
    }

    NSString *retVal = [[[_data objectAtIndex:indexPath.section] objectAtIndex:indexPath.row] valueForKey:@"sectionHeader"];
    
    [myLabel setText:[NSString stringWithFormat:@"    %@", retVal]];
    [header addSubview:myLabel];
    return header;
}

- (NSInteger)numberOfSectionsInCollectionView: (UICollectionView *)collectionView {
    return [_data count];
}

- (NSInteger)collectionView:(UICollectionView *)view numberOfItemsInSection:(NSInteger)section {
    return [[_data objectAtIndex:section] count];
}


- (void)showImageView:(NSArray*)arguments withDict:(NSDictionary*)optionsoptions
{
	if(nil == _collectionView){
        [self createImageView:nil withDict:nil];
	}
	
	if(NO == [_collectionView isHidden]){
		return;
	}
    
    
	
	_originalWebViewFrame = self.webView.frame;
	
	CGRect mainTableFrame;
	mainTableFrame = CGRectMake(
                                _originalWebViewFrame.origin.x,
                                _originalWebViewFrame.origin.y + _navbar.frame.size.height,
                                _originalWebViewFrame.size.width,
                                _originalWebViewFrame.size.height - _navbar.frame.size.height
                                );
	
	[_collectionView setFrame:mainTableFrame];
	[_collectionView setHidden:NO];
    [_navbar setHidden:NO];

    [self fadeIn];

    
}
- (void)hideImageView:(NSArray *)arguments withDict:(NSDictionary *)options
{
    //    [self searchBarCancelButtonClicked:_searchBar];
    [[self networkQueue] cancelAllOperations];
    
	if(nil == _collectionView){
        return;
	}
	
	if(YES == [_collectionView isHidden]){
		return;
	}
    
    if (concurrentQueue) {
        //dispatch_release(concurrentQueue);
    }
    
    [_searchBar resignFirstResponder];
    
    
    [self fadeOut];
	
	[self.webView setFrame:_originalWebViewFrame];
    
	
}


- (IBAction)onBackButtonPress:(id)sender
{
    NSString * jsCallBack = @"window.plugins.ImageCollection._onBackButtonTap();";
    [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
}

- (IBAction)onRightButtonPress:(id)sender
{
    NSString * jsCallBack = @"window.plugins.ImageCollection._onRightButtonTap();";
    [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
}

- (IBAction)onEdit:(id)sender
{
    if (isEditing) {
        _navbar.topItem.rightBarButtonItem.title = @"Edit";
        _navbar.topItem.rightBarButtonItem.style = UIBarButtonSystemItemDone;
        isEditing = NO;
    } else {
        _navbar.topItem.rightBarButtonItem.title = @"Done";
        _navbar.topItem.rightBarButtonItem.style = UIBarButtonSystemItemEdit;
        isEditing = YES;
    }

    [_collectionView reloadData];
}

- (IBAction)doEditMode:(id)sender
{

    _navbar.topItem.rightBarButtonItem.title = @"Done";
    _navbar.topItem.rightBarButtonItem.style = UIBarButtonSystemItemEdit;
    isEditing = YES;

    [_collectionView reloadData];
    [self startQuivering];
}


- (IBAction)onDelete:(id)sender event:(id)event {
    NSSet *touches = [event allTouches];
    UITouch *touch = [touches anyObject];
    CGPoint currentTouchPosition = [touch locationInView:_collectionView];
    NSIndexPath *indexPath = [_collectionView indexPathForItemAtPoint: currentTouchPosition];
    
	NSDictionary *item =[[_data objectAtIndex:indexPath.section] objectAtIndex:indexPath.row];
    int actualRow = [[item valueForKey:@"JSIndex"] intValue];
    UIBAlertView *alert = [[UIBAlertView alloc] initWithTitle:@"picServe" message:@"Are you sure you want to delete?"cancelButtonTitle:@"Delete" otherButtonTitles:@"Cancel",nil];
    
    [alert showWithDismissHandler:^(NSInteger selectedIndex, BOOL didCancel) {
        if (selectedIndex == 0 ) {
            NSString * jsCallBack = [NSString stringWithFormat:@"window.plugins.ImageCollection._onRightButtonTap(%d);", actualRow];
            [self.webView stringByEvaluatingJavaScriptFromString:jsCallBack];
            [[_data objectAtIndex:indexPath.section] removeObject:item];
            [_collectionView reloadData];
        }
    }];

}

-(void)fadeIn
{
    [self resizeView];
    
    CGRect r = [_collectionView frame];
    r.origin.y = [_collectionView frame].size.height;
    [_collectionView setFrame:r];
    _collectionView.alpha = 0;
    _navbar.alpha = 0;
    [_navbar setHidden:NO];
    [_collectionView setHidden:NO];
    
    
    [UIView animateWithDuration:0.3
                          delay: 0.0
                        options:UIViewAnimationCurveEaseInOut
                     animations:^{
                         CGRect r = [_collectionView frame];
                         r.origin.y = _navbar.frame.size.height;
                         [_collectionView setFrame:r];
                         _collectionView.alpha = 1;
                         _navbar.alpha =1 ;
                     }
                     completion:^(BOOL finished){
                         [self.webView stringByEvaluatingJavaScriptFromString:@"window.plugins.ImageCollection._onTableShowComplete();"];
                         [self loadImagesForOnscreenRows];
                     }];
    
}

-(void)fadeOut
{
    
    
    CGRect r = [_collectionView frame];
    r.origin.y = _offsetTop;
    [_collectionView setFrame:r];
    _collectionView.alpha = 1;
    _navbar.alpha = 1;
    [_navbar setHidden:NO];
    [_collectionView setHidden:NO];
    
    [UIView animateWithDuration:0.3
                          delay: 0.0
                        options:UIViewAnimationCurveEaseInOut
                     animations:^{
                         CGRect r = [_collectionView frame];
                         r.origin.y = [_collectionView frame].size.height;
                         [_collectionView setFrame:r];
                         _collectionView.alpha = 0;
                         _navbar.alpha = 0;
                     }
                     completion:^(BOOL finished){
                    
                         [_navbar setHidden:YES];
                         [_collectionView setHidden:YES];
                         [self.webView stringByEvaluatingJavaScriptFromString:@"window.plugins.ImageCollection._onTableHideComplete();"];
                         
                     }];
    
    
}

- (void)startQuivering
{

    
}
- (void)stopQuivering
{
    CALayer *layer = _collectionView.layer;
    [layer removeAnimationForKey:@"quivering"];
}




- (void)requestFailed:(ASIHTTPRequest *)request
{

    //... Handle failure
    //set image to whatever instead..?
   // NSLog(@"Request failed");
}


- (void)queueFinished:(ASINetworkQueue *)queue
{
     //NSLog(@"queue Done!");
   // [_collectionView reloadData];
}

@end


